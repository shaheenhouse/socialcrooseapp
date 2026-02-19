"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import Image from "next/image";
import Link from "next/link";
import {
  Store,
  Plus,
  Edit,
  Eye,
  Package,
  Star,
  TrendingUp,
  DollarSign,
  Users,
  Settings,
  MoreHorizontal,
  Search,
  Filter,
  BarChart3,
  ShoppingBag,
  Clock,
  CheckCircle2,
  XCircle,
  Pause,
  Play,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Progress } from "@/components/ui/progress";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

const storeData = {
  name: "Ahmed's Digital Services",
  slug: "ahmeddev",
  avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Ahmed",
  banner: "https://images.unsplash.com/photo-1579547621113-e4bb2a19bdd6?w=1200&h=400&fit=crop",
  description: "Professional web development and design services",
  rating: 4.9,
  reviews: 142,
  completedOrders: 234,
  responseTime: "1 hour",
  stats: {
    totalRevenue: 2450000,
    thisMonth: 234500,
    totalOrders: 234,
    activeOrders: 8,
    views: 15678,
  },
};

const services = [
  {
    id: "s1",
    title: "Professional Website Development",
    image: "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=400&h=300&fit=crop",
    price: 500,
    orders: 89,
    rating: 4.9,
    reviews: 78,
    status: "active",
    views: 2345,
    impressions: 12456,
  },
  {
    id: "s2",
    title: "E-commerce Store Setup",
    image: "https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=400&h=300&fit=crop",
    price: 800,
    orders: 45,
    rating: 4.8,
    reviews: 42,
    status: "active",
    views: 1876,
    impressions: 9234,
  },
  {
    id: "s3",
    title: "Mobile App Development",
    image: "https://images.unsplash.com/photo-1551650975-87deedd944c3?w=400&h=300&fit=crop",
    price: 1500,
    orders: 23,
    rating: 5.0,
    reviews: 21,
    status: "active",
    views: 1234,
    impressions: 7654,
  },
  {
    id: "s4",
    title: "API Integration Services",
    image: "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=400&h=300&fit=crop",
    price: 300,
    orders: 67,
    rating: 4.7,
    reviews: 56,
    status: "paused",
    views: 987,
    impressions: 5432,
  },
];

const recentOrders = [
  {
    id: "o1",
    buyer: {
      name: "Tech Solutions Inc",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=TS",
    },
    service: "Professional Website Development",
    amount: 85000,
    status: "in-progress",
    date: "Dec 1, 2025",
  },
  {
    id: "o2",
    buyer: {
      name: "StartUp Pro",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=SP",
    },
    service: "Mobile App Development",
    amount: 150000,
    status: "pending",
    date: "Nov 30, 2025",
  },
  {
    id: "o3",
    buyer: {
      name: "MediCare Plus",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=MC",
    },
    service: "E-commerce Store Setup",
    amount: 80000,
    status: "delivered",
    date: "Nov 28, 2025",
  },
];

const statusConfig: Record<string, { label: string; color: string; icon: React.ElementType }> = {
  active: {
    label: "Active",
    color: "bg-green-500/10 text-green-500",
    icon: CheckCircle2,
  },
  paused: {
    label: "Paused",
    color: "bg-amber-500/10 text-amber-500",
    icon: Pause,
  },
  draft: {
    label: "Draft",
    color: "bg-gray-500/10 text-gray-500",
    icon: Clock,
  },
};

const orderStatusColors: Record<string, string> = {
  pending: "bg-gray-500/10 text-gray-500",
  "in-progress": "bg-blue-500/10 text-blue-500",
  delivered: "bg-purple-500/10 text-purple-500",
  completed: "bg-green-500/10 text-green-500",
  cancelled: "bg-red-500/10 text-red-500",
};

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: {
    opacity: 1,
    y: 0,
    transition: { duration: 0.4 },
  },
};

function formatCurrency(amount: number) {
  if (amount >= 1000000) {
    return `${(amount / 1000000).toFixed(1)}M`;
  }
  if (amount >= 1000) {
    return `${(amount / 1000).toFixed(0)}K`;
  }
  return amount.toLocaleString();
}

export default function StorePage() {
  const [filterStatus, setFilterStatus] = useState("all");

  const filteredServices = services.filter(
    (service) => filterStatus === "all" || service.status === filterStatus
  );

  return (
    <div className="space-y-6">
      {/* Store header */}
      <Card className="overflow-hidden">
        <div className="relative h-32 w-full">
          <Image
            src={storeData.banner}
            alt="Store banner"
            fill
            className="object-cover"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />
        </div>
        <CardContent className="relative -mt-8 pb-6">
          <div className="flex flex-col items-center gap-4 sm:flex-row sm:items-end">
            <Avatar className="h-24 w-24 border-4 border-background">
              <AvatarImage src={storeData.avatar} />
              <AvatarFallback>AD</AvatarFallback>
            </Avatar>
            <div className="flex-1 text-center sm:text-left">
              <h1 className="text-2xl font-bold">{storeData.name}</h1>
              <p className="text-muted-foreground">@{storeData.slug}</p>
              <div className="mt-2 flex flex-wrap items-center justify-center gap-4 text-sm sm:justify-start">
                <div className="flex items-center gap-1">
                  <Star className="h-4 w-4 fill-amber-400 text-amber-400" />
                  <span className="font-medium">{storeData.rating}</span>
                  <span className="text-muted-foreground">
                    ({storeData.reviews} reviews)
                  </span>
                </div>
                <span className="text-muted-foreground">
                  {storeData.completedOrders} orders completed
                </span>
              </div>
            </div>
            <div className="flex gap-2">
              <Button variant="outline">
                <Eye className="mr-2 h-4 w-4" />
                Preview Store
              </Button>
              <Button variant="outline">
                <Settings className="mr-2 h-4 w-4" />
                Settings
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-5">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-full bg-green-500/10 p-2">
                <DollarSign className="h-5 w-5 text-green-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total Revenue</p>
                <p className="text-xl font-bold">
                  PKR {formatCurrency(storeData.stats.totalRevenue)}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-full bg-blue-500/10 p-2">
                <TrendingUp className="h-5 w-5 text-blue-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">This Month</p>
                <p className="text-xl font-bold">
                  PKR {formatCurrency(storeData.stats.thisMonth)}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-full bg-purple-500/10 p-2">
                <Package className="h-5 w-5 text-purple-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total Orders</p>
                <p className="text-xl font-bold">{storeData.stats.totalOrders}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-full bg-amber-500/10 p-2">
                <ShoppingBag className="h-5 w-5 text-amber-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Active Orders</p>
                <p className="text-xl font-bold">{storeData.stats.activeOrders}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-full bg-pink-500/10 p-2">
                <Eye className="h-5 w-5 text-pink-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Store Views</p>
                <p className="text-xl font-bold">
                  {formatCurrency(storeData.stats.views)}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="services" className="w-full">
        <TabsList>
          <TabsTrigger value="services">My Services</TabsTrigger>
          <TabsTrigger value="orders">Recent Orders</TabsTrigger>
          <TabsTrigger value="analytics">Analytics</TabsTrigger>
        </TabsList>

        {/* Services Tab */}
        <TabsContent value="services" className="mt-6 space-y-4">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="relative flex-1 sm:max-w-md">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input placeholder="Search services..." className="pl-9" />
            </div>
            <div className="flex items-center gap-2">
              <Select value={filterStatus} onValueChange={setFilterStatus}>
                <SelectTrigger className="w-[150px]">
                  <SelectValue placeholder="Status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="active">Active</SelectItem>
                  <SelectItem value="paused">Paused</SelectItem>
                  <SelectItem value="draft">Draft</SelectItem>
                </SelectContent>
              </Select>
              <Button>
                <Plus className="mr-2 h-4 w-4" />
                Add Service
              </Button>
            </div>
          </div>

          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className="grid gap-4 md:grid-cols-2 lg:grid-cols-3"
          >
            {filteredServices.map((service) => {
              const status = statusConfig[service.status];
              const StatusIcon = status.icon;

              return (
                <motion.div key={service.id} variants={itemVariants}>
                  <Card className="overflow-hidden transition-all hover:shadow-lg">
                    <div className="relative aspect-video">
                      <Image
                        src={service.image}
                        alt={service.title}
                        fill
                        className="object-cover"
                      />
                      <Badge
                        variant="outline"
                        className={cn(
                          "absolute left-2 top-2",
                          status.color
                        )}
                      >
                        <StatusIcon className="mr-1 h-3 w-3" />
                        {status.label}
                      </Badge>
                    </div>
                    <CardContent className="p-4">
                      <h3 className="line-clamp-1 font-semibold">
                        {service.title}
                      </h3>
                      <p className="mt-1 text-lg font-bold text-primary">
                        PKR {service.price.toLocaleString()}
                      </p>

                      <div className="mt-3 grid grid-cols-2 gap-2 text-sm">
                        <div className="flex items-center gap-1 text-muted-foreground">
                          <Package className="h-4 w-4" />
                          {service.orders} orders
                        </div>
                        <div className="flex items-center gap-1 text-muted-foreground">
                          <Eye className="h-4 w-4" />
                          {formatCurrency(service.views)} views
                        </div>
                        <div className="flex items-center gap-1">
                          <Star className="h-4 w-4 fill-amber-400 text-amber-400" />
                          {service.rating}
                        </div>
                        <div className="text-muted-foreground">
                          {service.reviews} reviews
                        </div>
                      </div>
                    </CardContent>
                    <CardFooter className="flex justify-between border-t p-3">
                      <Button variant="ghost" size="sm">
                        <Edit className="mr-1 h-4 w-4" />
                        Edit
                      </Button>
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="icon">
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem>
                            <Eye className="mr-2 h-4 w-4" />
                            Preview
                          </DropdownMenuItem>
                          <DropdownMenuItem>
                            <BarChart3 className="mr-2 h-4 w-4" />
                            Analytics
                          </DropdownMenuItem>
                          {service.status === "active" ? (
                            <DropdownMenuItem>
                              <Pause className="mr-2 h-4 w-4" />
                              Pause
                            </DropdownMenuItem>
                          ) : (
                            <DropdownMenuItem>
                              <Play className="mr-2 h-4 w-4" />
                              Activate
                            </DropdownMenuItem>
                          )}
                          <DropdownMenuItem className="text-destructive">
                            <XCircle className="mr-2 h-4 w-4" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </CardFooter>
                  </Card>
                </motion.div>
              );
            })}

            {/* Add service card */}
            <motion.div variants={itemVariants}>
              <Card className="flex h-full min-h-[300px] cursor-pointer items-center justify-center border-dashed transition-all hover:border-primary hover:bg-muted/50">
                <div className="text-center">
                  <Plus className="mx-auto h-8 w-8 text-muted-foreground" />
                  <p className="mt-2 font-medium">Add New Service</p>
                  <p className="text-sm text-muted-foreground">
                    Create a new service listing
                  </p>
                </div>
              </Card>
            </motion.div>
          </motion.div>
        </TabsContent>

        {/* Orders Tab */}
        <TabsContent value="orders" className="mt-6">
          <Card>
            <CardHeader>
              <CardTitle>Recent Orders</CardTitle>
              <CardDescription>
                Your latest customer orders
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {recentOrders.map((order) => (
                  <div
                    key={order.id}
                    className="flex items-center justify-between rounded-lg border p-4"
                  >
                    <div className="flex items-center gap-4">
                      <Avatar>
                        <AvatarImage src={order.buyer.avatar} />
                        <AvatarFallback>{order.buyer.name[0]}</AvatarFallback>
                      </Avatar>
                      <div>
                        <p className="font-medium">{order.buyer.name}</p>
                        <p className="text-sm text-muted-foreground">
                          {order.service}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-4">
                      <div className="text-right">
                        <p className="font-semibold">
                          PKR {order.amount.toLocaleString()}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          {order.date}
                        </p>
                      </div>
                      <Badge
                        variant="outline"
                        className={orderStatusColors[order.status]}
                      >
                        {order.status === "in-progress"
                          ? "In Progress"
                          : order.status.charAt(0).toUpperCase() +
                            order.status.slice(1)}
                      </Badge>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
            <CardFooter className="justify-center">
              <Button variant="outline">View All Orders</Button>
            </CardFooter>
          </Card>
        </TabsContent>

        {/* Analytics Tab */}
        <TabsContent value="analytics" className="mt-6">
          <Card>
            <CardContent className="flex flex-col items-center justify-center py-12">
              <BarChart3 className="h-12 w-12 text-muted-foreground/50" />
              <h3 className="mt-4 text-lg font-semibold">Analytics Coming Soon</h3>
              <p className="text-muted-foreground">
                Detailed analytics and insights about your store performance
              </p>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
