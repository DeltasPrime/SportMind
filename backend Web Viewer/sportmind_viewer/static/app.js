const App = (() => {
	const els = {
		datesList: null,
		sessionsList: null,
		selectedDate: null,
		refreshBtn: null
	};

	function el(tag, cls, text) {
		const e = document.createElement(tag);
		if (cls) e.className = cls;
		if (text) e.textContent = text;
		return e;
	}

	async function fetchJSON(url) {
		const res = await fetch(url);
		if (!res.ok) {
			const txt = await res.text();
			throw new Error(`HTTP ${res.status}: ${txt}`);
		}
		return res.json();
	}

	function renderDates(dates) {
		els.datesList.innerHTML = "";
		if (!dates || dates.length === 0) {
			els.datesList.appendChild(el("li", "item", "Sin fechas"));
			return;
		}
		dates.forEach((d) => {
			const item = el("li", "item");
			const left = el("div");
			left.appendChild(el("div", null, d));
			left.appendChild(el("div", "meta", "Click para ver sesiones"));

			const btn = el("a", "link", "Ver sesiones");
			btn.href = "#";
			btn.addEventListener("click", (ev) => {
				ev.preventDefault();
				loadSessions(d);
			});

			item.appendChild(left);
			item.appendChild(btn);
			els.datesList.appendChild(item);
		});
	}

	function renderSessions(date, sessions) {
		els.selectedDate.textContent = date || "—";
		els.sessionsList.innerHTML = "";
		if (!sessions || sessions.length === 0) {
			els.sessionsList.appendChild(el("li", "item", "Sin sesiones"));
			return;
		}
		
		// Cambiar a grid
		els.sessionsList.className = "sessions-grid";
		
		sessions.forEach((s) => {
			// Extraer datos del usuario desde s.data o directamente de s
			const userData = s.data || s;
			const playerName = userData.playerName || "Sin nombre";
			const sessionId = s.session_id || "N/A";
			const timestamp = s.timestamp ? new Date(s.timestamp).toLocaleString('es-ES') : "Sin fecha";
			
			// Crear tarjeta
			const card = el("div", "session-card");
			
			// Header con nombre destacado
			const header = el("div", "card-header");
			const nameEl = el("div", "player-name", playerName);
			const sessionIdBadge = el("div", "session-badge-card");
			sessionIdBadge.textContent = `Sesión: ${sessionId.substring(0, 12)}...`;
			header.appendChild(nameEl);
			header.appendChild(sessionIdBadge);
			
			// Body con datos principales
			const body = el("div", "card-body");
			
			// Timestamp
			const timeRow = el("div", "data-row");
			timeRow.appendChild(el("span", "data-label", "Fecha:"));
			timeRow.appendChild(el("span", "data-value", timestamp));
			body.appendChild(timeRow);
			
			// Deporte seleccionado
			if (userData.selectedSport) {
				const sportRow = el("div", "data-row");
				sportRow.appendChild(el("span", "data-label", "Deporte:"));
				sportRow.appendChild(el("span", "data-value", userData.selectedSport));
				body.appendChild(sportRow);
			}
			
			// Género
			if (userData.gender) {
				const genderRow = el("div", "data-row");
				genderRow.appendChild(el("span", "data-label", "Género:"));
				genderRow.appendChild(el("span", "data-value", userData.gender));
				body.appendChild(genderRow);
			}
			
			// Estado emocional
			if (userData.emotionalState) {
				const emotionRow = el("div", "data-row");
				emotionRow.appendChild(el("span", "data-label", "Estado emocional:"));
				emotionRow.appendChild(el("span", "data-value", userData.emotionalState));
				body.appendChild(emotionRow);
			}
			
			// Footer con botón
			const footer = el("div", "card-footer");
			const btn = el("button", "btn-view-json", "Ver detalles completos");
			btn.addEventListener("click", (ev) => {
				ev.preventDefault();
				showDetailedModal(s);
			});
			footer.appendChild(btn);
			
			// Ensamblar tarjeta
			card.appendChild(header);
			card.appendChild(body);
			card.appendChild(footer);
			
			els.sessionsList.appendChild(card);
		});
	}

	async function loadDates() {
		try {
			const data = await fetchJSON("/api/dates");
			renderDates(data.dates || []);
		} catch (err) {
			console.error(err);
			alert("Error cargando fechas");
		}
	}

	async function loadSessions(date) {
		try {
			const data = await fetchJSON(`/api/sessions/${encodeURIComponent(date)}`);
			renderSessions(date, data.sessions || []);
		} catch (err) {
			console.error(err);
			alert("Error cargando sesiones");
		}
	}

	function showDetailedModal(session) {
		const userData = session.data || session;
		const sessionId = session.session_id || "N/A";
		const timestamp = session.timestamp ? new Date(session.timestamp).toLocaleString('es-ES') : "Sin fecha";
		
		const modal = el("div", "json-modal");
		
		// Header del modal
		const header = el("div", "json-modal-header");
		const headerLeft = el("div", "modal-header-left");
		const sessionBadge = el("div", "session-badge");
		sessionBadge.textContent = `Sesión: ${sessionId.substring(0, 12)}...`;
		headerLeft.appendChild(sessionBadge);
		headerLeft.appendChild(el("div", "modal-timestamp", timestamp));
		header.appendChild(headerLeft);
		
		const closeBtn = el("button", "json-modal-close");
		closeBtn.textContent = "×";
		closeBtn.addEventListener("click", () => {
			document.body.removeChild(modal);
		});
		header.appendChild(closeBtn);
		
		// Contenido del modal
		const content = el("div", "json-modal-content");
		
		// Función helper para crear metric card
		function createMetricCard(label, value, highlight = false) {
			const card = el("div", "metric-card");
			const labelEl = el("div", "metric-label", label);
			const valueEl = el("div", highlight ? "metric-value highlight" : "metric-value", value);
			card.appendChild(labelEl);
			card.appendChild(valueEl);
			return card;
		}
		
		// 1. Layout superior: Información del Usuario y Estado de Regulación Emocional (lado a lado)
		const topLayout = el("div", "top-info-layout");
		
		// Columna izquierda: Información del Usuario
		const userSection = el("div", "metrics-section user-info-section");
		userSection.appendChild(el("h3", "section-title", "Información del Usuario"));
		
		const userGrid = el("div", "metrics-grid");
		
		if (userData.playerName) {
			userGrid.appendChild(createMetricCard("Nombre", userData.playerName, true));
		}
		if (userData.selectedSport) {
			userGrid.appendChild(createMetricCard("Deporte Seleccionado", userData.selectedSport));
		}
		if (userData.gender) {
			userGrid.appendChild(createMetricCard("Género", userData.gender));
		}
		
		if (userGrid.children.length > 0) {
			userSection.appendChild(userGrid);
		}
		topLayout.appendChild(userSection);
		
		// Columna derecha: Estado de Regulación Emocional
		const regulationSection = el("div", "metrics-section regulation-section");
		regulationSection.appendChild(el("h3", "section-title", "Estado de Regulación Emocional"));
		
		const regulationContent = el("div", "regulation-content");
		
		// Obtener predicción de autorregulación emocional
		const emotionalRegulation = session.emotional_regulation;
		
		if (emotionalRegulation) {
			const isRegulated = emotionalRegulation.prediction === 1;
			const label = emotionalRegulation.label || (isRegulated ? "Sí" : "No");
			const confidence = emotionalRegulation.confidence;
			
			// Crear contenedor principal
			const regulationResult = el("div", "regulation-result");
			regulationResult.className = `regulation-result ${isRegulated ? "regulated" : "not-regulated"}`;
			
			// Icono y texto principal
			const mainResult = el("div", "regulation-main");
			const icon = el("span", "regulation-icon");
			icon.textContent = isRegulated ? "✅" : "❌";
			mainResult.appendChild(icon);
			
			const labelEl = el("span", "regulation-label");
			labelEl.textContent = label;
			mainResult.appendChild(labelEl);
			
			regulationResult.appendChild(mainResult);
			
			// Mostrar confianza si está disponible
			if (confidence !== null && confidence !== undefined) {
				const confidenceEl = el("div", "regulation-confidence");
				const confidencePercent = (confidence * 100).toFixed(1);
				confidenceEl.textContent = `Confianza: ${confidencePercent}%`;
				regulationResult.appendChild(confidenceEl);
			}
			
			// Descripción
			const description = el("div", "regulation-description");
			description.textContent = isRegulated 
				? "El usuario muestra capacidad de autorregulación emocional"
				: "El usuario requiere apoyo para la autorregulación emocional";
			regulationResult.appendChild(description);
			
			regulationContent.appendChild(regulationResult);
		} else {
			// Si no hay predicción disponible
			const regulationMessage = el("div", "regulation-message");
			regulationMessage.textContent = "Análisis no disponible";
			regulationContent.appendChild(regulationMessage);
		}
		
		regulationSection.appendChild(regulationContent);
		topLayout.appendChild(regulationSection);
		
		content.appendChild(topLayout);
		
		// 2. Layout principal: Métricas a la izquierda, Emociones a la derecha
		const mainLayout = el("div", "main-layout");
		
		// Columna izquierda: Métricas Principales
		const metricsColumn = el("div", "main-column");
		const metricsSection = el("div", "metrics-section");
		metricsSection.appendChild(el("h3", "section-title", "Métricas Principales"));
		
		const metricsColumns = el("div", "metrics-columns");
		
		// Columna de Tiro 🎯
		const shootingColumn = el("div", "metric-column");
		const shootingTitle = el("h4", "column-title");
		shootingTitle.textContent = "🎯 Tiro al Blanco";
		shootingColumn.appendChild(shootingTitle);
		
		const shootingGrid = el("div", "metrics-grid");
		if (userData.shootingScoreEasy !== undefined) {
			shootingGrid.appendChild(createMetricCard("Puntuación Fácil", userData.shootingScoreEasy.toFixed(1)));
		}
		if (userData.shootingScoreHard !== undefined) {
			shootingGrid.appendChild(createMetricCard("Puntuación Difícil", userData.shootingScoreHard.toFixed(1)));
		}
		if (userData.shootingRendimiento !== undefined) {
			shootingGrid.appendChild(createMetricCard("Rendimiento", userData.shootingRendimiento, false));
		}
		if (userData.shootingRitmo !== undefined) {
			shootingGrid.appendChild(createMetricCard("Ritmo", userData.shootingRitmo));
		}
		if (userData.shootingConfianza !== undefined) {
			shootingGrid.appendChild(createMetricCard("Confianza", userData.shootingConfianza));
		}
		shootingColumn.appendChild(shootingGrid);
		
		// Columna de Escalada 🧗
		const climbingColumn = el("div", "metric-column");
		const climbingTitle = el("h4", "column-title");
		climbingTitle.textContent = "🧗 Muro de Escalada";
		climbingColumn.appendChild(climbingTitle);
		
		const climbingGrid = el("div", "metrics-grid");
		if (userData.climbingTimeEasy !== undefined) {
			climbingGrid.appendChild(createMetricCard("Tiempo Fácil", `${userData.climbingTimeEasy.toFixed(1)}s`));
		}
		if (userData.climbingTimeHard !== undefined) {
			climbingGrid.appendChild(createMetricCard("Tiempo Difícil", `${userData.climbingTimeHard.toFixed(1)}s`));
		}
		if (userData.climbingRendimiento !== undefined) {
			climbingGrid.appendChild(createMetricCard("Rendimiento", userData.climbingRendimiento, false));
		}
		if (userData.climbingRitmo !== undefined) {
			climbingGrid.appendChild(createMetricCard("Ritmo", userData.climbingRitmo));
		}
		if (userData.climbingConfianza !== undefined) {
			climbingGrid.appendChild(createMetricCard("Confianza", userData.climbingConfianza));
		}
		climbingColumn.appendChild(climbingGrid);
		
		// Columna de General
		const generalColumn = el("div", "metric-column");
		const generalTitle = el("h4", "column-title");
		generalTitle.textContent = "📊 General";
		generalColumn.appendChild(generalTitle);
		
		const generalGrid = el("div", "metrics-grid");
		if (userData.recomendacionFinal !== undefined) {
			generalGrid.appendChild(createMetricCard("Recomendación Final", userData.recomendacionFinal, false));
		}
		generalColumn.appendChild(generalGrid);
		
		metricsColumns.appendChild(shootingColumn);
		metricsColumns.appendChild(climbingColumn);
		if (generalGrid.children.length > 0) {
			metricsColumns.appendChild(generalColumn);
		}
		
		metricsSection.appendChild(metricsColumns);
		metricsColumn.appendChild(metricsSection);
		mainLayout.appendChild(metricsColumn);
		
		// Columna derecha: Estados Emocionales
		const emotionsColumn = el("div", "main-column");
		const emotionsSection = el("div", "metrics-section");
		emotionsSection.appendChild(el("h3", "section-title", "Estados Emocionales"));
		
		const emotionsColumns = el("div", "metrics-columns");
		
		// Columna de Tiro 🎯
		const shootingEmotionsColumn = el("div", "metric-column");
		const shootingEmotionsTitle = el("h4", "column-title");
		shootingEmotionsTitle.textContent = "🎯 Tiro al Blanco";
		shootingEmotionsColumn.appendChild(shootingEmotionsTitle);
		
		const shootingEmotionsGrid = el("div", "metrics-grid");
		if (userData.preEmotionTiroEasy) {
			shootingEmotionsGrid.appendChild(createMetricCard("Pre-Emoción Fácil", userData.preEmotionTiroEasy));
		}
		if (userData.preEmotionTiroHard) {
			shootingEmotionsGrid.appendChild(createMetricCard("Pre-Emoción Difícil", userData.preEmotionTiroHard));
		}
		if (userData.shootingPostEmotion) {
			shootingEmotionsGrid.appendChild(createMetricCard("Post-Emoción", userData.shootingPostEmotion));
		}
		shootingEmotionsColumn.appendChild(shootingEmotionsGrid);
		
		// Columna de Escalada 🧗
		const climbingEmotionsColumn = el("div", "metric-column");
		const climbingEmotionsTitle = el("h4", "column-title");
		climbingEmotionsTitle.textContent = "🧗 Muro de Escalada";
		climbingEmotionsColumn.appendChild(climbingEmotionsTitle);
		
		const climbingEmotionsGrid = el("div", "metrics-grid");
		if (userData.preEmotionMuroEasy) {
			climbingEmotionsGrid.appendChild(createMetricCard("Pre-Emoción Fácil", userData.preEmotionMuroEasy));
		}
		if (userData.preEmotionMuroHard) {
			climbingEmotionsGrid.appendChild(createMetricCard("Pre-Emoción Difícil", userData.preEmotionMuroHard));
		}
		if (userData.climbingPostEmotion) {
			climbingEmotionsGrid.appendChild(createMetricCard("Post-Emoción", userData.climbingPostEmotion));
		}
		climbingEmotionsColumn.appendChild(climbingEmotionsGrid);
		
		// Columna de General
		const generalEmotionsColumn = el("div", "metric-column");
		const generalEmotionsTitle = el("h4", "column-title");
		generalEmotionsTitle.textContent = "😊 General";
		generalEmotionsColumn.appendChild(generalEmotionsTitle);
		
		const generalEmotionsGrid = el("div", "metrics-grid");
		if (userData.emotionalState) {
			generalEmotionsGrid.appendChild(createMetricCard("Estado Emocional Inicial", userData.emotionalState));
		}
		generalEmotionsColumn.appendChild(generalEmotionsGrid);
		
		emotionsColumns.appendChild(shootingEmotionsColumn);
		emotionsColumns.appendChild(climbingEmotionsColumn);
		if (generalEmotionsGrid.children.length > 0) {
			emotionsColumns.appendChild(generalEmotionsColumn);
		}
		
		emotionsSection.appendChild(emotionsColumns);
		emotionsColumn.appendChild(emotionsSection);
		mainLayout.appendChild(emotionsColumn);
		
		content.appendChild(mainLayout);
		
		// Botón para ver JSON raw (abre modal separado)
		const rawJsonSection = el("div", "raw-json-section");
		const toggleBtn = el("button", "btn-toggle-json", "Ver JSON Raw");
		
		toggleBtn.addEventListener("click", (ev) => {
			ev.preventDefault();
			showJsonModal(session);
		});
		
		rawJsonSection.appendChild(toggleBtn);
		content.appendChild(rawJsonSection);
		
		// Ensamblar modal
		const modalWrapper = el("div", "json-modal-wrapper");
		modalWrapper.appendChild(header);
		modalWrapper.appendChild(content);
		modal.appendChild(modalWrapper);
		
		document.body.appendChild(modal);
		
		// Cerrar al hacer clic fuera
		modal.addEventListener("click", (e) => {
			if (e.target === modal) {
				document.body.removeChild(modal);
			}
		});
	}

	function showJsonModal(session) {
		const modal = el("div", "json-modal");
		
		const modalWrapper = el("div", "json-modal-wrapper json-only-modal");
		
		// Header del modal
		const header = el("div", "json-modal-header");
		const headerTitle = el("h3", "json-modal-title");
		headerTitle.textContent = "JSON Completo";
		header.appendChild(headerTitle);
		
		const closeBtn = el("button", "json-modal-close");
		closeBtn.textContent = "×";
		closeBtn.addEventListener("click", () => {
			document.body.removeChild(modal);
		});
		header.appendChild(closeBtn);
		
		// Contenido JSON
		const jsonContent = el("pre", "json-content-full");
		jsonContent.textContent = JSON.stringify(session, null, 2);
		
		// Botón de copiar
		const copyBtn = el("button", "btn-copy-json");
		copyBtn.textContent = "📋 Copiar JSON";
		copyBtn.addEventListener("click", () => {
			navigator.clipboard.writeText(JSON.stringify(session, null, 2)).then(() => {
				copyBtn.textContent = "✅ Copiado!";
				setTimeout(() => {
					copyBtn.textContent = "📋 Copiar JSON";
				}, 2000);
			}).catch(() => {
				// Fallback para navegadores que no soportan clipboard API
				const textArea = document.createElement("textarea");
				textArea.value = JSON.stringify(session, null, 2);
				document.body.appendChild(textArea);
				textArea.select();
				document.execCommand("copy");
				document.body.removeChild(textArea);
				copyBtn.textContent = "✅ Copiado!";
				setTimeout(() => {
					copyBtn.textContent = "📋 Copiar JSON";
				}, 2000);
			});
		});
		
		const actionsBar = el("div", "json-actions-bar");
		actionsBar.appendChild(copyBtn);
		
		modalWrapper.appendChild(header);
		modalWrapper.appendChild(actionsBar);
		modalWrapper.appendChild(jsonContent);
		modal.appendChild(modalWrapper);
		
		document.body.appendChild(modal);
		
		// Cerrar al hacer clic fuera
		modal.addEventListener("click", (e) => {
			if (e.target === modal) {
				document.body.removeChild(modal);
			}
		});
		
		// Cerrar con ESC
		const escHandler = (e) => {
			if (e.key === "Escape") {
				document.body.removeChild(modal);
				document.removeEventListener("keydown", escHandler);
			}
		};
		document.addEventListener("keydown", escHandler);
	}

	function init() {
		els.datesList = document.getElementById("datesList");
		els.sessionsList = document.getElementById("sessionsList");
		els.selectedDate = document.getElementById("selectedDate");
		els.refreshBtn = document.getElementById("refreshDates");

		els.refreshBtn.addEventListener("click", loadDates);
		loadDates();
	}

	return { init };
})();


