import { create } from "zustand";
import { persist } from "zustand/middleware";
import { Locale, defaultLocale, localeDirections } from "@/i18n/config";

interface LanguageState {
  locale: Locale;
  direction: "ltr" | "rtl";
  setLocale: (locale: Locale) => void;
}

export const useLanguageStore = create<LanguageState>()(
  persist(
    (set) => ({
      locale: defaultLocale,
      direction: localeDirections[defaultLocale],
      setLocale: (locale: Locale) =>
        set({
          locale,
          direction: localeDirections[locale],
        }),
    }),
    {
      name: "language-storage",
    }
  )
);
