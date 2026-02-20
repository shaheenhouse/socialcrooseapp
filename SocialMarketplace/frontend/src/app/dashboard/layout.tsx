"use client";

import { useState, useEffect, useCallback, memo } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { motion, AnimatePresence } from "framer-motion";
import {
  LayoutDashboard,
  ShoppingBag,
  Store,
  Briefcase,
  FileText,
  CreditCard,
  MessageSquare,
  Bell,
  Settings,
  User,
  ChevronLeft,
  ChevronRight,
  LogOut,
  Moon,
  Sun,
  Search,
  Menu,
  X,
  Award,
  BarChart3,
  Wallet,
  Users,
  Building2,
  Globe,
  Palette,
  Newspaper,
  BookOpen,
  Receipt,
  Package,
  ScrollText,
  ShoppingCart,
  Heart,
  Tag,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import { Tooltip, TooltipContent, TooltipTrigger, TooltipProvider } from "@/components/ui/tooltip";
import { useTheme } from "next-themes";
import { useAuthStore } from "@/store/auth-store";
import { Badge } from "@/components/ui/badge";
import { SearchTrigger } from "@/components/global-search";
import { useNotificationStore } from "@/store/notification-store";
import { SignalRProvider } from "@/components/signalr-provider";

interface NavSection {
  title: string;
  items: NavItem[];
}

interface NavItem {
  name: string;
  href: string;
  icon: any;
  badge?: string;
}

const navSections: NavSection[] = [
  {
    title: "Main",
    items: [
      { name: "Dashboard", href: "/dashboard", icon: LayoutDashboard },
      { name: "Feed", href: "/dashboard/feed", icon: Newspaper },
      { name: "My Network", href: "/dashboard/network", icon: Users },
      { name: "Messages", href: "/dashboard/messages", icon: MessageSquare },
    ],
  },
  {
    title: "Business",
    items: [
      { name: "Marketplace", href: "/dashboard/marketplace", icon: ShoppingBag },
      { name: "My Store", href: "/dashboard/store", icon: Store },
      { name: "Inventory", href: "/dashboard/inventory", icon: Package },
      { name: "Orders", href: "/dashboard/orders", icon: CreditCard },
      { name: "Invoices", href: "/dashboard/invoices", icon: Receipt },
      { name: "Khata / Ledger", href: "/dashboard/khata", icon: BookOpen },
      { name: "Discounts", href: "/dashboard/discounts", icon: Tag },
    ],
  },
  {
    title: "Shopping",
    items: [
      { name: "Cart", href: "/dashboard/cart", icon: ShoppingCart },
      { name: "Wishlist", href: "/dashboard/wishlist", icon: Heart },
    ],
  },
  {
    title: "Work",
    items: [
      { name: "Projects", href: "/dashboard/projects", icon: Briefcase },
      { name: "Tenders", href: "/dashboard/tenders", icon: ScrollText },
      { name: "Skills & Tests", href: "/dashboard/skills", icon: Award },
    ],
  },
  {
    title: "Finance",
    items: [
      { name: "Wallet", href: "/dashboard/wallet", icon: Wallet },
      { name: "Analytics", href: "/dashboard/analytics", icon: BarChart3 },
    ],
  },
  {
    title: "Professional",
    items: [
      { name: "Company", href: "/dashboard/company", icon: Building2 },
      { name: "Team", href: "/dashboard/team", icon: Users },
      { name: "Portfolio", href: "/dashboard/portfolio", icon: Globe },
      { name: "Resumes", href: "/dashboard/resume", icon: FileText },
      { name: "Design Studio", href: "/dashboard/designs", icon: Palette },
    ],
  },
];

const bottomNavigation = [
  { name: "Notifications", href: "/dashboard/notifications", icon: Bell },
  { name: "Settings", href: "/dashboard/settings", icon: Settings },
];

const NavItemComponent = memo(function NavItemComponent({
  item,
  isActive,
  collapsed,
  onClick,
}: {
  item: NavItem;
  isActive: boolean;
  collapsed: boolean;
  onClick?: () => void;
}) {
  const content = (
    <Link
      href={item.href}
      onClick={onClick}
      className={cn(
        "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
        isActive
          ? "bg-primary text-primary-foreground shadow-sm"
          : "text-muted-foreground hover:bg-accent hover:text-foreground"
      )}
    >
      <item.icon className="h-4.5 w-4.5 flex-shrink-0" />
      {!collapsed && (
        <span className="flex-1 truncate">{item.name}</span>
      )}
      {!collapsed && item.badge && (
        <Badge
          variant={isActive ? "secondary" : "default"}
          className="ml-auto h-5 px-1.5 text-[10px]"
        >
          {item.badge}
        </Badge>
      )}
    </Link>
  );

  if (collapsed) {
    return (
      <Tooltip delayDuration={0}>
        <TooltipTrigger asChild>{content}</TooltipTrigger>
        <TooltipContent side="right" className="font-medium">
          {item.name}
        </TooltipContent>
      </Tooltip>
    );
  }

  return content;
});

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const [collapsed, setCollapsed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const [mounted, setMounted] = useState(false);
  const pathname = usePathname();
  const router = useRouter();
  const { theme, setTheme } = useTheme();
  const { user, logout, isAuthenticated } = useAuthStore();
  const { unreadCount, fetchUnreadCount } = useNotificationStore();

  useEffect(() => {
    setMounted(true);
    if (!isAuthenticated) {
      router.push("/auth/login");
    }
  }, [isAuthenticated, router]);

  useEffect(() => {
    if (isAuthenticated) fetchUnreadCount();
  }, [isAuthenticated, fetchUnreadCount]);

  // Close mobile sidebar on route change
  useEffect(() => {
    setMobileOpen(false);
  }, [pathname]);

  const handleLogout = useCallback(() => {
    logout();
    router.push("/");
  }, [logout, router]);

  const toggleTheme = useCallback(() => {
    setTheme(theme === "dark" ? "light" : "dark");
  }, [theme, setTheme]);

  if (!mounted) return null;

  return (
    <SignalRProvider>
    <TooltipProvider>
      <div className="min-h-screen bg-background">
        {/* Mobile overlay */}
        <AnimatePresence>
          {mobileOpen && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 z-40 bg-background/80 backdrop-blur-sm lg:hidden"
              onClick={() => setMobileOpen(false)}
            />
          )}
        </AnimatePresence>

        {/* Sidebar */}
        <aside
          className={cn(
            "fixed left-0 top-0 z-50 h-full border-r bg-card transition-all duration-200",
            "lg:translate-x-0",
            mobileOpen ? "translate-x-0" : "-translate-x-full lg:translate-x-0",
            collapsed ? "w-[68px]" : "w-[260px]"
          )}
        >
          {/* Logo */}
          <div className="flex h-14 items-center justify-between border-b px-3">
            {!collapsed && (
              <Link href="/dashboard" className="flex items-center gap-2">
                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-gradient-to-br from-primary to-purple-600 shadow-sm">
                  <ShoppingBag className="h-4.5 w-4.5 text-white" />
                </div>
                <span className="text-base font-bold tracking-tight">SocialMart</span>
              </Link>
            )}
            {collapsed && (
              <Link href="/dashboard" className="mx-auto">
                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-gradient-to-br from-primary to-purple-600 shadow-sm">
                  <ShoppingBag className="h-4.5 w-4.5 text-white" />
                </div>
              </Link>
            )}
            <Button
              variant="ghost"
              size="icon"
              className="hidden lg:flex h-7 w-7"
              onClick={() => setCollapsed(!collapsed)}
            >
              {collapsed ? <ChevronRight className="h-3.5 w-3.5" /> : <ChevronLeft className="h-3.5 w-3.5" />}
            </Button>
            <Button
              variant="ghost"
              size="icon"
              className="lg:hidden h-7 w-7"
              onClick={() => setMobileOpen(false)}
            >
              <X className="h-4 w-4" />
            </Button>
          </div>

          {/* Navigation with ScrollArea */}
          <div className="flex h-[calc(100vh-3.5rem)] flex-col">
            <ScrollArea className="flex-1 py-2">
              <nav className="px-2 space-y-4">
                {navSections.map((section) => (
                  <div key={section.title}>
                    {!collapsed && (
                      <div className="px-3 mb-1">
                        <span className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground/70">
                          {section.title}
                        </span>
                      </div>
                    )}
                    {collapsed && <Separator className="my-1" />}
                    <div className="space-y-0.5">
                      {section.items.map((item) => (
                        <NavItemComponent
                          key={item.href}
                          item={item}
                          isActive={pathname === item.href || (item.href !== "/dashboard" && pathname.startsWith(item.href))}
                          collapsed={collapsed}
                          onClick={() => setMobileOpen(false)}
                        />
                      ))}
                    </div>
                  </div>
                ))}
              </nav>
            </ScrollArea>

            {/* Bottom navigation */}
            <div className="border-t p-2 space-y-0.5">
              {bottomNavigation.map((item) => (
                <NavItemComponent
                  key={item.href}
                  item={item}
                  isActive={pathname === item.href}
                  collapsed={collapsed}
                />
              ))}
            </div>
          </div>
        </aside>

        {/* Main content */}
        <div
          className={cn(
            "min-h-screen transition-all duration-200",
            collapsed ? "lg:pl-[68px]" : "lg:pl-[260px]"
          )}
        >
          {/* Top bar */}
          <header className="sticky top-0 z-30 flex h-14 items-center justify-between border-b bg-background/95 px-4 backdrop-blur-lg supports-[backdrop-filter]:bg-background/60 lg:px-6">
            <div className="flex items-center gap-3">
              <Button
                variant="ghost"
                size="icon"
                className="lg:hidden h-8 w-8"
                onClick={() => setMobileOpen(true)}
              >
                <Menu className="h-5 w-5" />
              </Button>
              <div className="hidden md:block">
                <SearchTrigger className="w-[280px] lg:w-[360px]" />
              </div>
              <Button variant="ghost" size="icon" className="md:hidden h-8 w-8">
                <Search className="h-4.5 w-4.5" />
              </Button>
            </div>

            <div className="flex items-center gap-1">
              {/* Theme toggle */}
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button variant="ghost" size="icon" className="h-8 w-8" onClick={toggleTheme}>
                    {theme === "dark" ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
                  </Button>
                </TooltipTrigger>
                <TooltipContent>{theme === "dark" ? "Light mode" : "Dark mode"}</TooltipContent>
              </Tooltip>

              {/* Notifications */}
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="relative h-8 w-8"
                    onClick={() => router.push("/dashboard/notifications")}
                  >
                    <Bell className="h-4 w-4" />
                    {unreadCount > 0 && (
                      <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-bold text-destructive-foreground">
                        {unreadCount > 99 ? "99+" : unreadCount}
                      </span>
                    )}
                  </Button>
                </TooltipTrigger>
                <TooltipContent>Notifications</TooltipContent>
              </Tooltip>

              {/* User menu */}
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" className="flex items-center gap-2 px-2 h-8">
                    <Avatar className="h-7 w-7">
                      <AvatarImage src={user?.avatarUrl} />
                      <AvatarFallback className="text-xs">
                        {user?.firstName?.[0]}{user?.lastName?.[0]}
                      </AvatarFallback>
                    </Avatar>
                    <span className="hidden md:inline-block text-sm font-medium">
                      {user?.firstName} {user?.lastName}
                    </span>
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56">
                  <DropdownMenuLabel className="font-normal">
                    <div className="flex flex-col space-y-1">
                      <p className="text-sm font-medium">{user?.firstName} {user?.lastName}</p>
                      <p className="text-xs text-muted-foreground">{user?.email}</p>
                    </div>
                  </DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem asChild>
                    <Link href="/dashboard/profile">
                      <User className="mr-2 h-4 w-4" /> Profile
                    </Link>
                  </DropdownMenuItem>
                  <DropdownMenuItem asChild>
                    <Link href="/dashboard/settings">
                      <Settings className="mr-2 h-4 w-4" /> Settings
                    </Link>
                  </DropdownMenuItem>
                  <DropdownMenuItem asChild>
                    <Link href="/dashboard/wallet">
                      <Wallet className="mr-2 h-4 w-4" /> Wallet
                    </Link>
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={handleLogout} className="text-red-600 focus:text-red-600">
                    <LogOut className="mr-2 h-4 w-4" /> Log out
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
          </header>

          {/* Page content */}
          <main className="p-4 lg:p-6">{children}</main>
        </div>
      </div>
    </TooltipProvider>
    </SignalRProvider>
  );
}
