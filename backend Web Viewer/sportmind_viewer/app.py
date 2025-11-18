import os
import json
import datetime
from typing import Optional, Dict, Any

from flask import Flask, jsonify, send_from_directory
from flask_cors import CORS
import boto3
from botocore.exceptions import ClientError
import requests
import joblib
import pandas as pd
import numpy as np

try:
	# Cargar variables desde env.example renombrado a .env si existe
	from dotenv import load_dotenv  # type: ignore
	load_dotenv()
except Exception:
	pass

# Cargar el modelo de autorregulación emocional
MODEL_PATH = os.path.join(os.path.dirname(__file__), "models", "modelo_autoregulacion_emocional.pkl")
modelo_autoregulacion = None

def load_model():
	"""Carga el modelo de autorregulación emocional"""
	global modelo_autoregulacion
	try:
		if os.path.exists(MODEL_PATH):
			modelo_autoregulacion = joblib.load(MODEL_PATH)
			print(f"✅ Modelo cargado exitosamente desde {MODEL_PATH}")
		else:
			print(f"⚠️  Advertencia: No se encontró el modelo en {MODEL_PATH}")
	except Exception as e:
		print(f"❌ Error al cargar el modelo: {str(e)}")
		modelo_autoregulacion = None

def predict_emotional_regulation(session_data: Dict[str, Any]) -> Optional[Dict[str, Any]]:
	"""
	Predice la autorregulación emocional basándose en los datos de la sesión.
	
	Args:
		session_data: Diccionario con los datos de la sesión (puede tener 'data' anidado)
	
	Returns:
		Dict con 'prediction' (0 o 1), 'label' ('No' o 'Sí'), y 'confidence' (opcional)
		o None si no se puede hacer la predicción
	"""
	if modelo_autoregulacion is None:
		return None
	
	try:
		# Extraer los datos del usuario (pueden estar en session_data.data o directamente en session_data)
		user_data = session_data.get('data', session_data)
		
		# Verificar si el modelo es un diccionario con 'modelo', 'scaler', etc. o solo el modelo
		if isinstance(modelo_autoregulacion, dict):
			model = modelo_autoregulacion.get('modelo')
			scaler = modelo_autoregulacion.get('scaler')
			encoders = modelo_autoregulacion.get('encoders', {})
			features = modelo_autoregulacion.get('features', [])
		else:
			# Si es solo el modelo, asumimos que ya está preprocesado
			model = modelo_autoregulacion
			scaler = None
			encoders = {}
			features = []
		
		if model is None:
			return None
		
		# Extraer características según el README del modelo
		datos = {
			'selectedSport': user_data.get('selectedSport', ''),
			'gender': user_data.get('gender', ''),
			'emotionalState': user_data.get('emotionalState', ''),
			'preEmotionTiroEasy': user_data.get('preEmotionTiroEasy', '0'),
			'preEmotionTiroHard': user_data.get('preEmotionTiroHard', '0'),
			'preEmotionMuroEasy': user_data.get('preEmotionMuroEasy', '0'),
			'preEmotionMuroHard': user_data.get('preEmotionMuroHard', '0'),
			'shootingScoreEasy': float(user_data.get('shootingScoreEasy', 0) or 0),
			'shootingScoreHard': float(user_data.get('shootingScoreHard', 0) or 0),
			'shootingRendimiento': int(user_data.get('shootingRendimiento', 0) or 0),
			'shootingRitmo': int(user_data.get('shootingRitmo', 0) or 0),
			'shootingConfianza': int(user_data.get('shootingConfianza', 0) or 0),
			'shootingPostEmotion': user_data.get('shootingPostEmotion', ''),
			'climbingTimeEasy': float(user_data.get('climbingTimeEasy', 0) or 0),
			'climbingTimeHard': float(user_data.get('climbingTimeHard', 0) or 0),
			'climbingRendimiento': int(user_data.get('climbingRendimiento', 0) or 0),
			'climbingRitmo': int(user_data.get('climbingRitmo', 0) or 0),
			'climbingConfianza': int(user_data.get('climbingConfianza', 0) or 0),
			'climbingPostEmotion': user_data.get('climbingPostEmotion', ''),
			'recomendacionFinal': int(user_data.get('recomendacionFinal', 0) or 0),
		}
		
		# Convertir a DataFrame
		df = pd.DataFrame([datos])
		
		# PASO 1: Convertir emociones pre-actividad a numéricas
		for col in ['preEmotionTiroEasy', 'preEmotionTiroHard', 
					'preEmotionMuroEasy', 'preEmotionMuroHard']:
			df[col] = pd.to_numeric(df[col], errors='coerce').fillna(3)
		
		# PASO 2: CODIFICAR variables categóricas y crear columnas con sufijo "_encoded"
		# El modelo espera que los encoders estén en un diccionario con nombres específicos
		if encoders:
			try:
				# Mapeo de nombres de encoders según el archivo de referencia
				le_sport = encoders.get('le_sport')
				le_gender = encoders.get('le_gender')
				le_emotionalState = encoders.get('le_emotionalState')
				le_post = encoders.get('le_post')
				
				if le_sport:
					df['selectedSport_encoded'] = le_sport.transform(df['selectedSport'])
				if le_gender:
					df['gender_encoded'] = le_gender.transform(df['gender'])
				if le_emotionalState:
					df['emotionalState_encoded'] = le_emotionalState.transform(df['emotionalState'])
				if le_post:
					df['shootingPostEmotion_encoded'] = le_post.transform(df['shootingPostEmotion'])
					df['climbingPostEmotion_encoded'] = le_post.transform(df['climbingPostEmotion'])
			except Exception as e:
				print(f"⚠️  Error al aplicar encoders: {str(e)}")
				return None
		
		# PASO 3: Seleccionar features en el orden correcto (si está disponible)
		if features and len(features) > 0:
			# Verificar que todas las features existan en el DataFrame
			missing_features = [f for f in features if f not in df.columns]
			if missing_features:
				print(f"⚠️  Features faltantes en el DataFrame: {missing_features}")
				return None
			X = df[features]
		else:
			# Si no hay lista de features, usar todas las columnas numéricas y las codificadas
			# Excluir las columnas categóricas originales
			numeric_cols = df.select_dtypes(include=[np.number]).columns.tolist()
			encoded_cols = [col for col in df.columns if col.endswith('_encoded')]
			X = df[numeric_cols + encoded_cols]
		
		# PASO 4: Aplicar scaler (después de codificar y seleccionar features)
		if scaler is not None:
			try:
				X_scaled = scaler.transform(X)
			except Exception as e:
				print(f"⚠️  Error al aplicar scaler: {str(e)}")
				return None
		else:
			X_scaled = X.values
		
		# PASO 5: Hacer la predicción
		prediction = model.predict(X_scaled)[0]
		
		# Intentar obtener probabilidades si el modelo las soporta
		confidence = None
		try:
			if hasattr(model, 'predict_proba'):
				proba = model.predict_proba(X_scaled)[0]
				confidence = float(max(proba))
		except:
			pass
		
		# Convertir predicción a formato legible
		prediction_label = "Sí" if prediction == 1 else "No"
		
		return {
			'prediction': int(prediction),
			'label': prediction_label,
			'confidence': confidence
		}
		
	except Exception as e:
		print(f"❌ Error al hacer predicción: {str(e)}")
		import traceback
		traceback.print_exc()
		return None


def create_app() -> Flask:
	app = Flask(__name__, static_folder="static", static_url_path="/static")
	CORS(app, resources={r"/api/*": {"origins": "*"}})

	# Cargar el modelo al iniciar la aplicación
	load_model()

	aws_region = os.getenv("AWS_DEFAULT_REGION", os.getenv("AWS_REGION", "sa-east-1"))
	s3_bucket = os.getenv("S3_BUCKET_NAME", "userdata-sportmind")

	s3 = boto3.client("s3", region_name=aws_region)

	@app.get("/")
	def index():
		return send_from_directory(app.static_folder, "index.html")

	@app.get("/api/health")
	def health():
		return jsonify({"ok": True})

	@app.get("/api/dates")
	def list_dates():
		"""
		Lista fechas desde S3 (por defecto) o, si está activado, desde API Gateway.
		"""
		use_api = os.getenv("USE_API_GATEWAY", "false").lower() == "true"
		if use_api:
			api_base = os.getenv("API_GATEWAY_BASE_URL", "").rstrip("/")
			api_key = os.getenv("API_GATEWAY_API_KEY", "")
			url = f"{api_base}/data/dates"
			try:
				headers = {}
				if api_key:
					headers["x-api-key"] = api_key
				r = requests.get(url, headers=headers, timeout=15)
				r.raise_for_status()
				data = r.json()
				return jsonify({"dates": data.get("dates", []), "total": data.get("total_dates", 0), "source": "api", "success": True})
			except Exception as e:
				return jsonify({"success": False, "error": str(e)}), 500

		# Por defecto: leer directo desde S3
		try:
			resp = s3.list_objects_v2(Bucket=s3_bucket, Prefix="data/", Delimiter="/")
			dates = []
			for prefix in resp.get("CommonPrefixes", []):
				p = prefix["Prefix"]
				if p.startswith("data/") and p.endswith("/"):
					dates.append(p[5:-1])
			dates.sort(reverse=True)
			return jsonify({"dates": dates, "total": len(dates), "source": "s3", "success": True})
		except ClientError as e:
			return jsonify({"success": False, "error": str(e)}), 500

	@app.get("/api/sessions/<date_str>")
	def get_sessions_by_date(date_str: str):
		"""
		Devuelve las sesiones JSON para una fecha YYYY-MM-DD (S3 por defecto o API Gateway si se habilita).
		"""
		try:
			datetime.datetime.strptime(date_str, "%Y-%m-%d")
		except ValueError:
			return jsonify({"success": False, "error": "Formato de fecha inválido. Use YYYY-MM-DD"}), 400

		use_api = os.getenv("USE_API_GATEWAY", "false").lower() == "true"
		if use_api:
			api_base = os.getenv("API_GATEWAY_BASE_URL", "").rstrip("/")
			api_key = os.getenv("API_GATEWAY_API_KEY", "")
			url = f"{api_base}/data/{date_str}"
			try:
				headers = {}
				if api_key:
					headers["x-api-key"] = api_key
				r = requests.get(url, headers=headers, timeout=20)
				r.raise_for_status()
				data = r.json()
				sessions = data.get("sessions", [])
				
				# Agregar predicción de autorregulación emocional a cada sesión
				for session in sessions:
					prediction = predict_emotional_regulation(session)
					if prediction:
						session["emotional_regulation"] = prediction
				
				return jsonify({
					"date": data.get("date", date_str),
					"total_sessions": data.get("total_sessions", 0),
					"sessions": sessions,
					"source": "api",
					"success": True
				})
			except Exception as e:
				return jsonify({"success": False, "error": str(e)}), 500

		prefix = f"data/{date_str}/"
		try:
			resp = s3.list_objects_v2(Bucket=s3_bucket, Prefix=prefix)
			sessions = []
			for item in resp.get("Contents", []):
				key = item["Key"]
				if not key.endswith(".json"):
					continue
				obj = s3.get_object(Bucket=s3_bucket, Key=key)
				txt = obj["Body"].read().decode("utf-8")
				try:
					data = json.loads(txt)
				except json.JSONDecodeError:
					continue
				data["_s3_key"] = key
				
				# Agregar predicción de autorregulación emocional
				prediction = predict_emotional_regulation(data)
				if prediction:
					data["emotional_regulation"] = prediction
				
				sessions.append(data)

			sessions.sort(key=lambda x: x.get("timestamp", ""), reverse=True)
			return jsonify({
				"date": date_str,
				"total_sessions": len(sessions),
				"sessions": sessions,
				"source": "s3",
				"success": True
			})
		except ClientError as e:
			return jsonify({"success": False, "error": str(e)}), 500

	return app


if __name__ == "__main__":
	app = create_app()
	app.run(host="0.0.0.0", port=5000, debug=True)


