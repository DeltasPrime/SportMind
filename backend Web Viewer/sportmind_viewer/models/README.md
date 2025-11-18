# Modelos de Machine Learning

Esta carpeta contiene los modelos entrenados para predecir la autorregulación emocional.

## Instrucciones

1. Coloca tu modelo entrenado en esta carpeta con el nombre `modelo_autoregulacion_emocional.pkl` (o el formato que uses: `.joblib`, `.h5`, etc.)

2. El modelo debe estar entrenado para recibir las siguientes características:
   - **Datos del paciente:**
     - selectedSport (string, codificado: deporte seleccionado)
     - gender (string, codificado: género del paciente)
     - emotionalState (string, codificado: estado emocional inicial)
   
   - **Datos de emociones pre-actividad:**
     - preEmotionTiroEasy (float, escala 1-5)
     - preEmotionTiroHard (float, escala 1-5)
     - preEmotionMuroEasy (float, escala 1-5)
     - preEmotionMuroHard (float, escala 1-5)
   
   - **Datos de actividad de tiro:**
     - shootingScoreEasy (float, puntuación en tiro fácil)
     - shootingScoreHard (float, puntuación en tiro difícil)
     - shootingRendimiento (int, escala 1-5)
     - shootingRitmo (int, escala 1-5)
     - shootingConfianza (int, escala 1-5)
     - shootingPostEmotion (string, codificado: emoción después del tiro)
   
   - **Datos de actividad de escalada:**
     - climbingTimeEasy (float, tiempo en segundos para escalada fácil)
     - climbingTimeHard (float, tiempo en segundos para escalada difícil)
     - climbingRendimiento (int, escala 1-5)
     - climbingRitmo (int, escala 1-5)
     - climbingConfianza (int, escala 1-5)
     - climbingPostEmotion (string, codificado: emoción después de la escalada)
   
   - **Datos adicionales:**
     - recomendacionFinal (int, recomendación final)

3. El modelo debe tener un método `predict()` que devuelva la autorregulación emocional (0 = No, 1 = Sí).

4. Si usas scikit-learn, guarda el modelo con:
   ```python
   import joblib
   modelo_completo = {
       'modelo': modelo,
       'scaler': scaler,
       'encoders': encoders,
       'features': features,
       'version': '1.0',
       'metadata': {
           'fecha_entrenamiento': pd.Timestamp.now().isoformat(),
           'tipo_modelo': type(modelo).__name__,
           'num_features': len(features),
           'accuracy': float(accuracy)
       }
   }
   joblib.dump(modelo_completo, 'models/modelo_autoregulacion_emocional.pkl')
   ```

5. Si usas TensorFlow/Keras, guarda el modelo con:
   ```python
   modelo.save('models/modelo_autoregulacion_emocional.h5')
   ```

## Formato de predicción esperado

El modelo debe devolver un valor que indique la autorregulación emocional. Puede ser:
- Un valor binario (0 = No, 1 = Sí)
- Una probabilidad (0.0-1.0)
- Una clase categórica ('No', 'Sí')

Ajusta el código en `app.py` según el formato de salida de tu modelo.

