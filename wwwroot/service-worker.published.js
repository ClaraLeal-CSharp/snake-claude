// Service Worker — produção (com cache offline completo)
// Gerado automaticamente pelo Blazor WASM durante publish; este é o template base.

const CACHE_NAME = 'snake-claude-v1';

self.addEventListener('install', event => {
    event.waitUntil(self.skipWaiting());
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys
                .filter(k => k !== CACHE_NAME)
                .map(k => caches.delete(k))
            )
        ).then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', event => {
    // Estratégia: cache-first para assets estáticos
    if (event.request.method !== 'GET') return;

    event.respondWith(
        caches.open(CACHE_NAME).then(async cache => {
            const cached = await cache.match(event.request);
            if (cached) return cached;

            try {
                const response = await fetch(event.request);
                if (response.ok) {
                    cache.put(event.request, response.clone());
                }
                return response;
            } catch {
                // Offline e sem cache — retorna página principal
                return cache.match('./') || new Response('Offline', { status: 503 });
            }
        })
    );
});
