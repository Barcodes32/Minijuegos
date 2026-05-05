/**
 * GameBridge.jslib
 * Plugin JavaScript para comunicación Unity ↔ React.
 * 
 * INSTALACIÓN:
 * Guardar en Assets/Plugins/WebGL/GameBridge.jslib
 * Unity lo incluye automáticamente en el build WebGL.
 * 
 * NOTA: Este es el mismo archivo de la Ruleta.
 * Si ya lo tienes, NO necesitas cambiarlo.
 */

mergeInto(LibraryManager.library, {

    /**
     * GetUserIdJS()
     * Obtiene el userId desde React (guardado en window por el componente React).
     * Llamado desde GameManager.cs al inicio.
     */
    GetUserIdJS: function() {
        var userId = window.unityUserId || 
                     window.__userId || 
                     localStorage.getItem('userId') || 
                     'anonymous';
        
        // Unity necesita que el string se copie a memoria WASM
        var bufferSize = lengthBytesUTF8(userId) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(userId, buffer, bufferSize);
        return buffer;
    },

    /**
     * DispatchGameEventJS(eventJson)
     * Dispara un CustomEvent de JavaScript que React escucha.
     * Llamado desde GameManager.cs cuando termina el juego.
     */
    DispatchGameEventJS: function(eventJsonPtr) {
        var eventJson = UTF8ToString(eventJsonPtr);
        
        try {
            var eventData = JSON.parse(eventJson);
            
            // Disparar evento global que el componente React escucha
            var event = new CustomEvent('unityGameComplete', {
                detail: eventData,
                bubbles: true
            });
            window.dispatchEvent(event);
            
            console.log('[GameBridge] Evento disparado:', eventData);
        } catch(e) {
            console.error('[GameBridge] Error al parsear evento:', e, eventJson);
        }
    },

    /**
     * LogToConsoleJS(messagePtr)
     * Utilidad para debug: imprime en consola del browser desde Unity.
     */
    LogToConsoleJS: function(messagePtr) {
        var message = UTF8ToString(messagePtr);
        console.log('[Unity]', message);
    }
});
