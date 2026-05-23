// ═══════════════════════════════════════════════════════════════
// snake-effects.js — Efeitos CRT, sons e interações do jogo
// Snake Claude — .NET 10 Blazor
// ═══════════════════════════════════════════════════════════════

window.SnakeEffects = (function () {
  'use strict';

  // ── Audio Context ─────────────────────────────────────────────
  let audioCtx = null;

  function getAudio() {
    if (!audioCtx) {
      try { audioCtx = new (window.AudioContext || window.webkitAudioContext)(); }
      catch (e) { return null; }
    }
    return audioCtx;
  }

  function beep(freq, dur, vol, type) {
    const ctx = getAudio();
    if (!ctx) return;
    try {
      const osc = ctx.createOscillator();
      const g   = ctx.createGain();
      osc.connect(g);
      g.connect(ctx.destination);
      osc.type = type || 'square';
      osc.frequency.setValueAtTime(freq, ctx.currentTime);
      g.gain.setValueAtTime(vol || 0.04, ctx.currentTime);
      g.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + dur);
      osc.start(ctx.currentTime);
      osc.stop(ctx.currentTime + dur);
    } catch (_) {}
  }

  // ── Controle de foco e teclado ────────────────────────────────
  const BLOCKED_KEYS = new Set([
    'ArrowUp','ArrowDown','ArrowLeft','ArrowRight','Space'
  ]);

  function attachKeyBlocker() {
    window.addEventListener('keydown', function (e) {
      if (BLOCKED_KEYS.has(e.code)) e.preventDefault();
    }, { passive: false });
  }

  return {
    // Inicializar: bloquear scroll + resumir AudioContext
    init: function () {
      attachKeyBlocker();
      document.addEventListener('click', function () {
        const ctx = getAudio();
        if (ctx && ctx.state === 'suspended') ctx.resume();
      }, { once: true });
    },

    // Focar o elemento do jogo para capturar teclado
    focusGame: function (elementId) {
      const el = document.getElementById(elementId || 'game-container');
      if (el) { el.focus({ preventScroll: true }); }
    },

    // ── Sons ──────────────────────────────────────────────────────
    playEat: function () {
      beep(440, 0.06, 0.05, 'square');
      setTimeout(() => beep(660, 0.06, 0.04, 'square'), 50);
    },

    playGameOver: function () {
      beep(220, 0.15, 0.06, 'sawtooth');
      setTimeout(() => beep(165, 0.2, 0.06, 'sawtooth'), 150);
      setTimeout(() => beep(110, 0.4, 0.05, 'sawtooth'), 320);
    },

    playStart: function () {
      beep(330, 0.07, 0.04, 'square');
      setTimeout(() => beep(440, 0.07, 0.04, 'square'), 90);
      setTimeout(() => beep(550, 0.10, 0.04, 'square'), 180);
    },

    playCombo: function (level) {
      const freq = 330 + (Math.min(level, 8) * 80);
      beep(freq, 0.09, 0.04, 'square');
    },

    playVictory: function () {
      const notes = [330, 440, 550, 660, 880];
      notes.forEach((f, i) => setTimeout(() => beep(f, 0.12, 0.05, 'square'), i * 100));
    },

    // Efeito visual de glitch breve
    triggerGlitch: function (id) {
      const el = document.getElementById(id || 'game-board');
      if (!el) return;
      el.style.filter = 'brightness(1.4) saturate(1.5)';
      el.style.transform = 'translateX(2px)';
      setTimeout(() => {
        el.style.filter = '';
        el.style.transform = '';
      }, 80);
    }
  };
})();

document.addEventListener('DOMContentLoaded', function () {
  SnakeEffects.init();
});
