import { getRequestConfig } from "next-intl/server";
import { defaultLocale, type Locale } from "./config";

export default getRequestConfig(async () => {
  // For now, we'll use cookie-based locale detection
  // In a real app, you might want to use URL-based routing
  const locale: Locale = defaultLocale;

  return {
    locale,
    messages: (await import(`./messages/${locale}.json`)).default,
  };
});
