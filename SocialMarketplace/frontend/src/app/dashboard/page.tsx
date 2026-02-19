"use client";

import { motion } from "framer-motion";
import {
  ArrowUpRight,
  ArrowDownRight,
  DollarSign,
  ShoppingBag,
  Users,
  TrendingUp,
  Package,
  Clock,
  Star,
  Eye,
  MoreHorizontal,
  ArrowRight,
  CheckCircle2,
  AlertCircle,
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

const stats = [
  {
    name: "Total Revenue",
    value: "$45,231.89",
    change: "+20.1%",
    trend: "up",
    icon: DollarSign,
  },
  {
    name: "Active Orders",
    value: "2,350",
    change: "+15.2%",
    trend: "up",
    icon: ShoppingBag,
  },
  {
    name: "New Customers",
    value: "+573",
    change: "+12.5%",
    trend: "up",
    icon: Users,
  },
  {
    name: "Conversion Rate",
    value: "3.2%",
    change: "-2.4%",
    trend: "down",
    icon: TrendingUp,
  },
];

const recentOrders = [
  {
    id: "ORD-001",
    customer: "John Doe",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=John",
    product: "Web Development Service",
    amount: "$1,200.00",
    status: "completed",
    date: "2 hours ago",
  },
  {
    id: "ORD-002",
    customer: "Jane Smith",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Jane",
    product: "Logo Design Package",
    amount: "$450.00",
    status: "pending",
    date: "4 hours ago",
  },
  {
    id: "ORD-003",
    customer: "Mike Johnson",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Mike",
    product: "SEO Optimization",
    amount: "$800.00",
    status: "processing",
    date: "6 hours ago",
  },
  {
    id: "ORD-004",
    customer: "Sarah Wilson",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Sarah",
    product: "Mobile App Development",
    amount: "$3,500.00",
    status: "completed",
    date: "1 day ago",
  },
  {
    id: "ORD-005",
    customer: "Alex Brown",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Alex",
    product: "Content Writing",
    amount: "$200.00",
    status: "cancelled",
    date: "1 day ago",
  },
];

const topProducts = [
  {
    name: "Web Development",
    sales: 234,
    revenue: "$45,600",
    progress: 85,
  },
  {
    name: "Mobile App Dev",
    sales: 189,
    revenue: "$38,200",
    progress: 72,
  },
  {
    name: "UI/UX Design",
    sales: 156,
    revenue: "$28,400",
    progress: 58,
  },
  {
    name: "SEO Services",
    sales: 142,
    revenue: "$22,100",
    progress: 52,
  },
];

const activeProjects = [
  {
    name: "E-commerce Platform",
    client: "Tech Corp",
    progress: 75,
    deadline: "Dec 15, 2025",
    status: "on-track",
  },
  {
    name: "Healthcare App",
    client: "MediCare Inc",
    progress: 45,
    deadline: "Jan 20, 2026",
    status: "on-track",
  },
  {
    name: "Banking Dashboard",
    client: "FinBank",
    progress: 90,
    deadline: "Dec 5, 2025",
    status: "at-risk",
  },
];

const statusColors: Record<string, string> = {
  completed: "bg-green-500/10 text-green-500 border-green-500/20",
  pending: "bg-yellow-500/10 text-yellow-500 border-yellow-500/20",
  processing: "bg-blue-500/10 text-blue-500 border-blue-500/20",
  cancelled: "bg-red-500/10 text-red-500 border-red-500/20",
};

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.1,
    },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: {
    opacity: 1,
    y: 0,
    transition: {
      duration: 0.5,
    },
  },
};

export default function DashboardPage() {
  return (
    <motion.div
      variants={containerVariants}
      initial="hidden"
      animate="visible"
      className="space-y-6"
    >
      {/* Page header */}
      <motion.div variants={itemVariants} className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Dashboard</h1>
          <p className="text-muted-foreground">
            Welcome back! Here&apos;s what&apos;s happening with your business.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline">Export</Button>
          <Button>
            <Package className="mr-2 h-4 w-4" />
            Add Product
          </Button>
        </div>
      </motion.div>

      {/* Stats grid */}
      <motion.div
        variants={itemVariants}
        className="grid gap-4 md:grid-cols-2 lg:grid-cols-4"
      >
        {stats.map((stat, index) => (
          <Card key={stat.name} className="relative overflow-hidden">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {stat.name}
              </CardTitle>
              <stat.icon className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.value}</div>
              <div className="flex items-center text-xs">
                {stat.trend === "up" ? (
                  <ArrowUpRight className="mr-1 h-4 w-4 text-green-500" />
                ) : (
                  <ArrowDownRight className="mr-1 h-4 w-4 text-red-500" />
                )}
                <span
                  className={
                    stat.trend === "up" ? "text-green-500" : "text-red-500"
                  }
                >
                  {stat.change}
                </span>
                <span className="ml-1 text-muted-foreground">from last month</span>
              </div>
            </CardContent>
            {/* Decorative gradient */}
            <div
              className={`absolute bottom-0 left-0 right-0 h-1 ${
                index === 0
                  ? "bg-gradient-to-r from-blue-500 to-purple-500"
                  : index === 1
                  ? "bg-gradient-to-r from-green-500 to-emerald-500"
                  : index === 2
                  ? "bg-gradient-to-r from-orange-500 to-amber-500"
                  : "bg-gradient-to-r from-pink-500 to-rose-500"
              }`}
            />
          </Card>
        ))}
      </motion.div>

      {/* Main content grid */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Recent orders */}
        <motion.div variants={itemVariants} className="lg:col-span-2">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <div>
                <CardTitle>Recent Orders</CardTitle>
                <CardDescription>
                  Your latest orders from customers
                </CardDescription>
              </div>
              <Button variant="ghost" size="sm">
                View All
                <ArrowRight className="ml-2 h-4 w-4" />
              </Button>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {recentOrders.map((order) => (
                  <div
                    key={order.id}
                    className="flex items-center justify-between rounded-lg border p-3 transition-colors hover:bg-muted/50"
                  >
                    <div className="flex items-center gap-3">
                      <Avatar className="h-10 w-10">
                        <AvatarImage src={order.avatar} />
                        <AvatarFallback>{order.customer[0]}</AvatarFallback>
                      </Avatar>
                      <div>
                        <p className="font-medium">{order.customer}</p>
                        <p className="text-sm text-muted-foreground">
                          {order.product}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-4">
                      <div className="text-right">
                        <p className="font-medium">{order.amount}</p>
                        <div className="flex items-center gap-1 text-xs text-muted-foreground">
                          <Clock className="h-3 w-3" />
                          {order.date}
                        </div>
                      </div>
                      <Badge
                        variant="outline"
                        className={statusColors[order.status]}
                      >
                        {order.status}
                      </Badge>
                      <Button variant="ghost" size="icon">
                        <MoreHorizontal className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </motion.div>

        {/* Top products */}
        <motion.div variants={itemVariants}>
          <Card>
            <CardHeader>
              <CardTitle>Top Services</CardTitle>
              <CardDescription>Your best performing services</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {topProducts.map((product) => (
                <div key={product.name} className="space-y-2">
                  <div className="flex items-center justify-between text-sm">
                    <span className="font-medium">{product.name}</span>
                    <span className="text-muted-foreground">
                      {product.sales} sales
                    </span>
                  </div>
                  <Progress value={product.progress} className="h-2" />
                  <div className="flex items-center justify-between text-xs text-muted-foreground">
                    <span>{product.revenue}</span>
                    <span>{product.progress}%</span>
                  </div>
                </div>
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
            <Button variant="outline" size="sm">
              View All Projects
            </Button>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-3">
              {activeProjects.map((project) => (
                <div
                  key={project.name}
                  className="rounded-lg border p-4 transition-all hover:shadow-md"
                >
                  <div className="mb-3 flex items-start justify-between">
                    <div>
                      <h4 className="font-semibold">{project.name}</h4>
                      <p className="text-sm text-muted-foreground">
                        {project.client}
                      </p>
                    </div>
                    {project.status === "on-track" ? (
                      <CheckCircle2 className="h-5 w-5 text-green-500" />
                    ) : (
                      <AlertCircle className="h-5 w-5 text-amber-500" />
                    )}
                  </div>
                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Progress</span>
                      <span className="font-medium">{project.progress}%</span>
                    </div>
                    <Progress value={project.progress} className="h-2" />
                  </div>
                  <div className="mt-3 flex items-center justify-between text-xs">
                    <span className="text-muted-foreground">Deadline</span>
                    <span className="font-medium">{project.deadline}</span>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </motion.div>

      {/* Quick actions */}
      <motion.div variants={itemVariants}>
        <Card className="bg-gradient-to-br from-primary/10 via-purple-500/10 to-pink-500/10">
          <CardContent className="flex flex-col items-center justify-between gap-4 p-6 md:flex-row">
            <div>
              <h3 className="text-lg font-semibold">Ready to grow your business?</h3>
              <p className="text-muted-foreground">
                Add new services, manage orders, and track your analytics all in one place.
              </p>
            </div>
            <div className="flex gap-2">
              <Button variant="outline">
                <Eye className="mr-2 h-4 w-4" />
                View Analytics
              </Button>
              <Button>
                <Star className="mr-2 h-4 w-4" />
                Upgrade Plan
              </Button>
            </div>
          </CardContent>
        </Card>
      </motion.div>
    </motion.div>
  );
}
