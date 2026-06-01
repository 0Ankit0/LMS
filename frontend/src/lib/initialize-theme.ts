import { THEME_PRESETS } from '@/lib/themes';

export function initializeTheme() {
  if (typeof document === 'undefined') {
    return;
  }

  try {
    const storedTheme = localStorage.getItem('theme-storage');
    let activeThemeId = THEME_PRESETS[0].id;
    let customThemes: unknown[] = [];

    if (storedTheme) {
      const parsed = JSON.parse(storedTheme) as {
        state?: { activeThemeId?: string; customThemes?: unknown[] };
      };

      if (parsed.state?.activeThemeId) {
        activeThemeId = parsed.state.activeThemeId;
      }

      if (Array.isArray(parsed.state?.customThemes)) {
        customThemes = parsed.state.customThemes;
      }
    }

    const themeList = [...THEME_PRESETS, ...customThemes] as typeof THEME_PRESETS;
    const activeTheme = themeList.find((theme) => theme.id === activeThemeId) ?? THEME_PRESETS[0];
    const root = document.documentElement;

    root.dataset.themeId = activeTheme.id;
    root.dataset.themeMode = activeTheme.mode;
    root.style.colorScheme = activeTheme.mode;

    Object.entries(activeTheme.palette).forEach(([key, value]) => {
      const cssName = key.replace(/[A-Z]/g, (letter) => `-${letter.toLowerCase()}`);
      root.style.setProperty(`--${cssName}`, value);
    });
  } catch {
    document.documentElement.dataset.themeMode = 'light';
  }
}