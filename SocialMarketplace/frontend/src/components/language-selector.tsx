"use client";

import { useState, useEffect } from "react";
import { Globe, Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useLanguageStore } from "@/store/language-store";
import { locales, localeNames, type Locale } from "@/i18n/config";
import { cn } from "@/lib/utils";

export function LanguageSelector() {
  const { locale, setLocale, direction } = useLanguageStore();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  useEffect(() => {
    // Update HTML dir and lang attributes when locale changes
    if (mounted) {
      document.documentElement.lang = locale;
      document.documentElement.dir = direction;
    }
  }, [locale, direction, mounted]);

  if (!mounted) {
    return (
      <Button variant="ghost" size="icon" disabled>
        <Globe className="h-5 w-5" />
      </Button>
    );
  }

  const handleLanguageChange = (newLocale: Locale) => {
    setLocale(newLocale);
    // In a real app, you might want to reload the page or fetch new translations
    window.location.reload();
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon">
          <Globe className="h-5 w-5" />
          <span className="sr-only">Change language</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-40">
        {locales.map((loc) => (
          <DropdownMenuItem
            key={loc}
            onClick={() => handleLanguageChange(loc)}
            className={cn(
              "flex items-center justify-between",
              locale === loc && "bg-accent"
            )}
          >
            <span>{localeNames[loc]}</span>
            {locale === loc && <Check className="h-4 w-4" />}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
