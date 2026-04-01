/* ===== CONNEXION WEBSOCKET =====
   window.location.host → donne automatiquement l'adresse du serveur
     qui a servi la page HTML
     Exemple : si tu ouvres http://abc123.ngrok-free.app
     alors host = "abc123.ngrok-free.app"
   
   window.location.protocol → "http:" ou "https:"
     Si https → on utilise wss:// (WebSocket securise)
     Si http  → on utilise ws:// (WebSocket normal)
   
   Comme ca, pas besoin de changer l'IP a la main a chaque fois */
const wsProtocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
const socket = new WebSocket(wsProtocol + '//' + window.location.host);
