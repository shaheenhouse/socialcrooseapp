export const locales = ["en", "ur", "ar", "zh", "es"] as const;
export type Locale = (typeof locales)[number];

export const defaultLocale: Locale = "en";

export const localeNames: Record<Locale, string> = {
  en: "English",
  ur: "اردو",
  ar: "العربية",
  zh: "中文",
  es: "Español",
};

export const localeDirections: Record<Locale, "ltr" | "rtl"> = {
  en: "ltr",
  ur: "rtl",
  ar: "rtl",
  zh: "ltr",
  es: "ltr",
};
