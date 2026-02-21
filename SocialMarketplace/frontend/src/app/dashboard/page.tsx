"use client";

import { useEffect } from "react";
import { motion } from "framer-motion";
import {
  ArrowUpRight,
  DollarSign,
  ShoppingBag,
  Users,
  Briefcase,
  Package,
  Clock,
  ArrowRight,
  CheckCircle2,
  AlertCircle,
  Plus,
  Palette,
  FileText,
  Building2,
  FolderOpen,
  Store,
  Rocket,
} from "lucide-react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { useAuthStore } from "@/store/auth-store";
import { useQuery } from "@tanstack/react-query";
import { orderApi, walletApi, projectApi, storeApi, connectionApi } from "@/lib/api";
import Link from "next/link";
import { formatCurrency, formatRelativeTime } from "@/lib/utils";

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.5 } },
};

const statusColors: Record<string, string> = {
  Completed: "bg-green-500/10 text-green-500 border-green-500/20",
  Pending: "bg-yellow-500/10 text-yellow-500 border-yellow-500/20",
  Processing: "bg-blue-500/10 text-blue-500 border-blue-500/20",
  Cancelled: "bg-red-500/10 text-red-500 border-red-500/20",
  InProgress: "bg-blue-500/10 text-blue-500 border-blue-500/20",
  Active: "bg-green-500/10 text-green-500 border-green-500/20",
};

function StatSkeleton() {
  return (
    <Card className="relative overflow-hidden">
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-4 w-4" />
      </CardHeader>
      <CardContent>
        <Skeleton className="h-8 w-20 mb-2" />
        <Skeleton className="h-3 w-32" />
      </CardContent>
    </Card>
  );
}

export default function DashboardPage() {
  const { user } = useAuthStore();
  const isAdmin = user?.roles?.includes("Admin");
  const isSeller = user?.roles?.includes("Seller");

  const { data: ordersData, isLoading: ordersLoading } = useQuery({
    queryKey: ["my-orders"],
    queryFn: () => orderApi.getAll({ page: 1, pageSize: 5 }),
    enabled: !!user,
  });

  const { data: salesData, isLoading: salesLoading } = useQuery({
    queryKey: ["my-sales"],
    queryFn: () => orderApi.getSales({ page: 1, pageSize: 5 }),
    enabled: !!user && isSeller,
  });

  const { data: walletData, isLoading: walletLoading } = useQuery({
    queryKey: ["my-wallet"],
    queryFn: () => walletApi.getWallet(),
    enabled: !!user,
  });

  const { data: projectsData, isLoading: projectsLoading } = useQuery({
    queryKey: ["my-projects"],
    queryFn: () => projectApi.getMyProjects({ page: 1, pageSize: 5 }),
    enabled: !!user,
  });

  const { data: connectionsData } = useQuery({
    queryKey: ["connection-stats"],
    queryFn: () => connectionApi.getStats(),
    enabled: !!user,
  });

  const orders = ordersData?.data?.items ?? ordersData?.data ?? [];
  const sales = salesData?.data?.items ?? salesData?.data ?? [];
  const wallet = walletData?.data;
  const projects = projectsData?.data?.items ?? projectsData?.data ?? [];
  const connStats = connectionsData?.data;

  const recentOrders = isSeller && sales.length > 0 ? sales : orders;
  const totalOrders = Array.isArray(orders) ? orders.length : 0;
  const totalProjects = Array.isArray(projects) ? projects.length : 0;
  const walletBalance = wallet?.balance ?? 0;
  const totalConnections = connStats?.totalConnections ?? 0;

  const stats = [
    {
      name: "Wallet Balance",
      value: formatCurrency(walletBalance),
      icon: DollarSign,
      gradient: "from-blue-500 to-purple-500",
    },
    {
      name: isSeller ? "Orders Received" : "My Orders",
      value: isSeller ? (sales.length || 0).toString() : totalOrders.toString(),
      icon: ShoppingBag,
      gradient: "from-green-500 to-emerald-500",
    },
    {
      name: "Connections",
      value: totalConnections.toString(),
      icon: Users,
      gradient: "from-orange-500 to-amber-500",
    },
    {
      name: "Active Projects",
      value: totalProjects.toString(),
      icon: Briefcase,
      gradient: "from-pink-500 to-rose-500",
    },
  ];

  const isLoading = ordersLoading || walletLoading || projectsLoading;
  const hasNoData = totalOrders === 0 && totalProjects === 0 && walletBalance === 0;

  return (
    <motion.div
      variants={containerVariants}
      initial="hidden"
      animate="visible"
      className="space-y-6"
    >
      <motion.div variants={itemVariants} className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">
            Welcome back, {user?.firstName || "there"}
          </h1>
          <p className="text-muted-foreground">
            {hasNoData && !isLoading
              ? "Get started by exploring the marketplace or setting up your profile."
              : "Here's what's happening with your account."}
          </p>
        </div>
        {user?.roles && (
          <div className="flex items-center gap-2">
            {user.roles.map((role) => (
              <Badge key={role} variant="secondary">
                {role}
              </Badge>
            ))}
          </div>
        )}
      </motion.div>

      {/* Stats grid */}
      <motion.div
        variants={itemVariants}
        className="grid gap-4 md:grid-cols-2 lg:grid-cols-4"
      >
        {isLoading
          ? Array.from({ length: 4 }).map((_, i) => <StatSkeleton key={i} />)
          : stats.map((stat) => (
              <Card key={stat.name} className="relative overflow-hidden">
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                  <CardTitle className="text-sm font-medium text-muted-foreground">
                    {stat.name}
                  </CardTitle>
                  <stat.icon className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{stat.value}</div>
                </CardContent>
                <div
                  className={`absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r ${stat.gradient}`}
                />
              </Card>
            ))}
      </motion.div>

      {/* New user getting started */}
      {hasNoData && !isLoading && (
        <motion.div variants={itemVariants}>
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Rocket className="h-5 w-5 text-primary" />
                Get Started
              </CardTitle>
              <CardDescription>
                Set up your workspace and start using the platform
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                {[
                  { title: "Build Your Portfolio", desc: "Showcase your work", icon: FolderOpen, href: "/dashboard/portfolio" },
                  { title: "Create a Resume", desc: "Professional templates", icon: FileText, href: "/dashboard/resume" },
                  { title: "Design Studio", desc: "Create visual assets", icon: Palette, href: "/dashboard/designs" },
                  { title: "Set Up Company", desc: "Manage your business", icon: Building2, href: "/dashboard/company" },
                ].map((item) => (
                  <Link key={item.href} href={item.href}>
                    <div className="flex flex-col items-center gap-3 rounded-lg border p-6 text-center transition-all hover:border-primary hover:shadow-md cursor-pointer">
                      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary/10">
                        <item.icon className="h-6 w-6 text-primary" />
                      </div>
                      <div>
                        <h4 className="font-semibold">{item.title}</h4>
                        <p className="text-sm text-muted-foreground">{item.desc}</p>
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            </CardContent>
          </Card>
        </motion.div>
      )}

      {/* Main content grid */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Recent orders */}
        <motion.div variants={itemVariants} className="lg:col-span-2">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <div>
                <CardTitle>
                  {isSeller ? "Recent Sales" : "Recent Orders"}
                </CardTitle>
                <CardDescription>
                  {isSeller
                    ? "Latest orders from your customers"
                    : "Your recent purchases and orders"}
                </CardDescription>
              </div>
              <Link href="/dashboard/orders">
                <Button variant="ghost" size="sm">
                  View All
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Button>
              </Link>
            </CardHeader>
            <CardContent>
              {ordersLoading || salesLoading ? (
                <div className="space-y-4">
                  {Array.from({ length: 3 }).map((_, i) => (
                    <div key={i} className="flex items-center gap-3 rounded-lg border p-3">
                      <Skeleton className="h-10 w-10 rounded-full" />
                      <div className="flex-1 space-y-2">
                        <Skeleton className="h-4 w-32" />
                        <Skeleton className="h-3 w-48" />
                      </div>
                    </div>
                  ))}
                </div>
              ) : recentOrders.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-12 text-center">
                  <ShoppingBag className="h-12 w-12 text-muted-foreground/50 mb-4" />
                  <h3 className="font-semibold text-lg">No orders yet</h3>
                  <p className="text-muted-foreground text-sm mt-1 max-w-sm">
                    {isSeller
                      ? "When customers place orders, they'll appear here."
                      : "Browse the marketplace to find services and products."}
                  </p>
                  <Link href="/dashboard/marketplace">
                    <Button className="mt-4" size="sm">
                      <Store className="mr-2 h-4 w-4" />
                      Browse Marketplace
                    </Button>
                  </Link>
                </div>
              ) : (
                <div className="space-y-4">
                  {recentOrders.slice(0, 5).map((order: any) => (
                    <div
                      key={order.id}
                      className="flex items-center justify-between rounded-lg border p-3 transition-colors hover:bg-muted/50"
                    >
                      <div className="flex items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10">
                          <Package className="h-5 w-5 text-primary" />
                        </div>
                        <div>
                          <p className="font-medium">
                            Order #{order.orderNumber || order.id?.slice(0, 8)}
                          </p>
                          <p className="text-sm text-muted-foreground">
                            {order.itemCount
                              ? `${order.itemCount} items`
                              : "Order"}
                          </p>
                        </div>
                      </div>
                      <div className="flex items-center gap-4">
                        <div className="text-right">
                          <p className="font-medium">
                            {formatCurrency(order.totalAmount || 0)}
                          </p>
                          <div className="flex items-center gap-1 text-xs text-muted-foreground">
                            <Clock className="h-3 w-3" />
                            {order.createdAt
                              ? formatRelativeTime(order.createdAt)
                              : "Recently"}
                          </div>
                        </div>
                        <Badge
                          variant="outline"
                          className={
                            statusColors[order.status] ??
                            "bg-gray-500/10 text-gray-500"
                          }
                        >
                          {order.status}
                        </Badge>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </motion.div>

        {/* Quick links */}
        <motion.div variants={itemVariants}>
          <Card>
            <CardHeader>
              <CardTitle>Quick Actions</CardTitle>
              <CardDescription>Frequently used features</CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {[
                { label: "Design Studio", icon: Palette, href: "/dashboard/designs", desc: "Create visual content" },
                { label: "My Portfolio", icon: FolderOpen, href: "/dashboard/portfolio", desc: "Manage your portfolio" },
                { label: "Resumes", icon: FileText, href: "/dashboard/resume", desc: "Build & manage resumes" },
                { label: "Companies", icon: Building2, href: "/dashboard/company", desc: "Manage businesses" },
                { label: "Marketplace", icon: Store, href: "/dashboard/marketplace", desc: "Browse & sell services" },
              ].map((item) => (
                <Link key={item.href} href={item.href}>
                  <div className="flex items-center gap-3 rounded-lg border p-3 transition-all hover:border-primary hover:bg-muted/50 cursor-pointer">
                    <div className="flex h-9 w-9 items-center justify-center rounded-md bg-primary/10">
                      <item.icon className="h-4 w-4 text-primary" />
                    </div>
                    <div className="flex-1">
                      <p className="font-medium text-sm">{item.label}</p>
                      <p className="text-xs text-muted-foreground">{item.desc}</p>
                    </div>
                    <ArrowRight className="h-4 w-4 text-muted-foreground" />
                  </div>
                </Link>
              ))}
            </CardContent>
          </Card>
        </motion.div>
      </div>

      {/* Active projects */}
      <motion.div variants={itemVariants}>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <div>
              <CardTitle>Active Projects</CardTitle>
              <CardDescription>Track your ongoing project milestones</CardDescription>
            </div>
            <Link href="/dashboard/projects">
              <Button variant="outline" size="sm">
                View All Projects
              </Button>
            </Link>
          </CardHeader>
          <CardContent>
            {projectsLoading ? (
              <div className="grid gap-4 md:grid-cols-3">
                {Array.from({ length: 3 }).map((_, i) => (
                  <div key={i} className="rounded-lg border p-4 space-y-3">
                    <Skeleton className="h-5 w-32" />
                    <Skeleton className="h-3 w-24" />
                    <Skeleton className="h-2 w-full" />
                  </div>
                ))}
              </div>
            ) : projects.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-12 text-center">
                <Briefcase className="h-12 w-12 text-muted-foreground/50 mb-4" />
                <h3 className="font-semibold text-lg">No active projects</h3>
                <p className="text-muted-foreground text-sm mt-1 max-w-sm">
                  Post a project or bid on existing ones to get started.
                </p>
                <Link href="/dashboard/projects">
                  <Button className="mt-4" size="sm">
                    <Plus className="mr-2 h-4 w-4" />
                    Browse Projects
                  </Button>
                </Link>
              </div>
            ) : (
              <div className="grid gap-4 md:grid-cols-3">
                {projects.slice(0, 6).map((project: any) => (
                  <div
                    key={project.id}
                    className="rounded-lg border p-4 transition-all hover:shadow-md"
                  >
                    <div className="mb-3 flex items-start justify-between">
                      <div>
                        <h4 className="font-semibold line-clamp-1">
                          {project.title || project.name}
                        </h4>
                        <p className="text-sm text-muted-foreground">
                          {project.clientName || project.categoryName || "Project"}
                        </p>
                      </div>
                      {project.status === "Active" ||
                      project.status === "InProgress" ? (
                        <CheckCircle2 className="h-5 w-5 text-green-500 shrink-0" />
                      ) : (
                        <AlertCircle className="h-5 w-5 text-amber-500 shrink-0" />
                      )}
                    </div>
                    {project.budget && (
                      <p className="text-sm font-medium">
                        Budget: {formatCurrency(project.budget)}
                      </p>
                    )}
                    <Badge
                      variant="outline"
                      className={`mt-2 ${
                        statusColors[project.status] ??
                        "bg-gray-500/10 text-gray-500"
                      }`}
                    >
                      {project.status}
                    </Badge>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </motion.div>
    </motion.div>
  );
}
