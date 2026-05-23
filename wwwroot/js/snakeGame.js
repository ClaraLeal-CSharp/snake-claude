// ============================================================
//  snakeGame.js  —  ES Module
//  Responsabilidades:
//    1. Captura de teclado (WASD + setas) e swipe touch
//    2. Ruído/faísca fosfórico no canvas CRT overlay
//    3. Prevenção de scroll da página nas teclas de jogo
// ============================================================

// ── Estado interno ──────────────────────────────────────────
let _dotnetRef   = null;
let _keyHandler  = null;
let _touchStartX = 0;
let _touchStartY = 0;

let _crtCanvas  = null;
let _crtCtx     = null;
let _crtFrame   = null;
let _noiseFrame = 0;

// ── Input ───────────────────────────────────────────────────
export function initInput(dotnetRef) {
    _dotnetRef = dotnetRef;

    const KEY_MAP = {
        ArrowUp:    'Up',    w: 'Up',    W: 'Up',
        ArrowDown:  'Down',  s: 'Down',  S: 'Down',
        ArrowLeft:  'Left',  a: 'Left',  A: 'Left',
        ArrowRight: 'Right', d: 'Right', D: 'Right',
        ' ':        'Pause', Escape: 'Pause',
        p:          'Pause', P: 'Pause',
    };

    const PREVENT_SCROLL = new Set(['ArrowUp','ArrowDown','ArrowLeft','ArrowRight',' ']);

    _keyHandler = (e) => {
        const action = KEY_MAP[e.key];
        if (!action) return;
        if (PREVENT_SCROLL.has(e.key)) e.preventDefault();
        _dotnetRef.invokeMethodAsync('HandleKeyInput', action);
    };

    document.addEventListener('keydown', _keyHandler);

    // ── Swipe touch ─────────────────────────────────────────
    document.addEventListener('touchstart', (e) => {
        _touchStartX = e.touches[0].clientX;
        _touchStartY = e.touches[0].clientY;
    }, { passive: true });

    document.addEventListener('touchend', (e) => {
        const dx  = e.changedTouches[0].clientX - _touchStartX;
        const dy  = e.changedTouches[0].clientY - _touchStartY;
        const adx = Math.abs(dx);
        const ady = Math.abs(dy);
        if (Math.max(adx, ady) < 28) return;       // swipe muito pequeno
        const dir = adx > ady ? (dx > 0 ? 'Right' : 'Left')
                               : (dy > 0 ? 'Down'  : 'Up');
        _dotnetRef.invokeMethodAsync('HandleKeyInput', dir);
    }, { passive: true });
}

// ── CRT Phosphor Noise ─────────────────────────────────────
export function initCrt(canvasId) {
    _crtCanvas = document.getElementById(canvasId);
    if (!_crtCanvas) return;
    _crtCtx = _crtCanvas.getContext('2d');
    _renderNoise();
}

function _renderNoise() {
    if (!_crtCanvas || !_crtCtx) return;

    const w = _crtCanvas.offsetWidth  | 0;
    const h = _crtCanvas.offsetHeight | 0;
    if (w === 0 || h === 0) {
        _crtFrame = requestAnimationFrame(_renderNoise);
        return;
    }
    if (_crtCanvas.width !== w)  _crtCanvas.width  = w;
    if (_crtCanvas.height !== h) _crtCanvas.height = h;

    _noiseFrame++;

    // Ruído a cada 4 frames (~15fps) para performance
    if (_noiseFrame % 4 === 0) {
        const img  = _crtCtx.createImageData(w, h);
        const data = img.data;
        for (let i = 0; i < data.length; i += 4) {
            const n = Math.random() * 14;
            data[i]     = n * 0.06;   // R — quase zero
            data[i + 1] = n;           // G — fosfórico
            data[i + 2] = n * 0.12;   // B — mínimo
            data[i + 3] = Math.random() < 0.003 ? 40 : (Math.random() * 8); // Alpha sutil
        }
        _crtCtx.putImageData(img, 0, 0);
    }

    // Flicker raríssimo — uma linha horizontal clara passando
    if (Math.random() < 0.0015) {
        const y = Math.random() * h | 0;
        _crtCtx.fillStyle = 'rgba(74,158,92,0.04)';
        _crtCtx.fillRect(0, y, w, 1);
    }

    _crtFrame = requestAnimationFrame(_renderNoise);
}

// ── Dispose ─────────────────────────────────────────────────
export function dispose() {
    if (_keyHandler) {
        document.removeEventListener('keydown', _keyHandler);
        _keyHandler = null;
    }
    if (_crtFrame) {
        cancelAnimationFrame(_crtFrame);
        _crtFrame = null;
    }
    _crtCanvas = null;
    _crtCtx    = null;
    _dotnetRef = null;
}
