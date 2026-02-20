import type { Metadata, Viewport } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';
import { ThemeProvider } from '@/components/theme-provider';
import { Toaster } from '@/components/ui/sonner';
import { QueryProvider } from '@/components/query-provider';

const inter = Inter({
  subsets: ['latin'],
  display: 'swap',
  preload: true,
});

export const metadata: Metadata = {
  title: {
    default: 'SocialMart - Connect, Trade, Grow',
    template: '%s | SocialMart',
  },
  description: 'A comprehensive social marketplace for freelancers, businesses, medical professionals, government entities, and all kinds of businesses. Buy, sell, connect, manage khata, invoices, and grow.',
  keywords: ['marketplace', 'freelance', 'services', 'products', 'projects', 'tenders', 'khata', 'ledger', 'business', 'medical', 'surgical', 'invoicing'],
  authors: [{ name: 'SocialMart' }],
  openGraph: {
    type: 'website',
    title: 'SocialMart - Connect, Trade, Grow',
    description: 'The all-in-one social marketplace for businesses of every kind.',
    siteName: 'SocialMart',
  },
};

export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1,
  maximumScale: 5,
  themeColor: [
    { media: '(prefers-color-scheme: light)', color: '#ffffff' },
    { media: '(prefers-color-scheme: dark)', color: '#0a0a1a' },
  ],
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={inter.className}>
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          <QueryProvider>
            {children}
            <Toaster position="top-right" richColors closeButton />
          </QueryProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
