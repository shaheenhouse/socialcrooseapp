"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import Image from "next/image";
import Link from "next/link";
import {
  Search,
  Filter,
  Clock,
  CheckCircle2,
  XCircle,
  AlertCircle,
  Package,
  Truck,
  Eye,
  MessageSquare,
  Star,
  MoreHorizontal,
  Calendar,
  ArrowUpRight,
  RefreshCw,
  FileText,
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const orders = [
  {
    id: "ORD-2025-001",
    type: "purchase",
    item: {
      name: "Professional Logo Design Package",
      image: "https://images.unsplash.com/photo-1626785774573-4b799315345d?w=100&h=100&fit=crop",
      category: "Design",
    },
    seller: {
      name: "Creative Studio",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Creative",
      rating: 4.9,
    },
    amount: 15000,
    status: "in-progress",
    orderDate: "Nov 28, 2025",
    deliveryDate: "Dec 5, 2025",
    progress: 65,
    milestones: [
      { name: "Initial Concepts", completed: true },
      { name: "Revisions", completed: true },
      { name: "Final Delivery", completed: false },
    ],
  },
  {
    id: "ORD-2025-002",
    type: "sale",
    item: {
      name: "Web Development Service",
      image: "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=100&h=100&fit=crop",
      category: "Development",
    },
    buyer: {
      name: "Tech Solutions Inc",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=TS",
    },
    amount: 85000,
    status: "delivered",
    orderDate: "Nov 20, 2025",
    deliveryDate: "Dec 1, 2025",
    progress: 100,
    awaitingReview: true,
  },
  {
    id: "ORD-2025-003",
    type: "purchase",
    item: {
      name: "SEO Optimization Package",
      image: "https://images.unsplash.com/photo-1432888622747-4eb9a8efeb07?w=100&h=100&fit=crop",
      category: "Marketing",
    },
    seller: {
      name: "SEO Masters",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=SEO",
      rating: 4.7,
    },
    amount: 25000,
    status: "completed",
    orderDate: "Nov 10, 2025",
    deliveryDate: "Nov 25, 2025",
    progress: 100,
    reviewed: true,
    rating: 5,
  },
  {
    id: "ORD-2025-004",
    type: "sale",
    item: {
      name: "Mobile App Development",
      image: "https://images.unsplash.com/photo-1551650975-87deedd944c3?w=100&h=100&fit=crop",
      category: "Development",
    },
    buyer: {
      name: "StartUp Pro",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=SP",
    },
    amount: 120000,
    status: "pending",
    orderDate: "Dec 1, 2025",
    deliveryDate: "Jan 15, 2026",
    progress: 10,
  },
  {
    id: "ORD-2025-005",
    type: "purchase",
    item: {
      name: "Video Editing Service",
      image: "https://images.unsplash.com/photo-1574717024453-354056aafa98?w=100&h=100&fit=crop",
      category: "Video",
    },
    seller: {
      name: "Video Pro",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Video",
      rating: 4.8,
    },
    amount: 8000,
    status: "cancelled",
    orderDate: "Nov 5, 2025",
    cancellationReason: "Seller unavailable",
  },
];

const statusConfig: Record<string, { label: string; color: string; icon: React.ElementType }> = {
  pending: {
    label: "Pending",
    color: "bg-gray-500/10 text-gray-500 border-gray-500/20",
    icon: Clock,
  },
  "in-progress": {
    label: "In Progress",
    color: "bg-blue-500/10 text-blue-500 border-blue-500/20",
    icon: RefreshCw,
  },
  delivered: {
    label: "Delivered",
    color: "bg-purple-500/10 text-purple-500 border-purple-500/20",
    icon: Package,
  },
  completed: {
    label: "Completed",
    color: "bg-green-500/10 text-green-500 border-green-500/20",
    icon: CheckCircle2,
  },
  cancelled: {
    label: "Cancelled",
    color: "bg-red-500/10 text-red-500 border-red-500/20",
    icon: XCircle,
  },
  disputed: {
    label: "Disputed",
    color: "bg-amber-500/10 text-amber-500 border-amber-500/20",
    icon: AlertCircle,
  },
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
  return new Intl.NumberFormat("en-PK", {
    style: "currency",
    currency: "PKR",
    minimumFractionDigits: 0,
  }).format(amount);
}

export default function OrdersPage() {
  const [filterStatus, setFilterStatus] = useState("all");
  const [searchQuery, setSearchQuery] = useState("");

  const filteredOrders = orders.filter((order) => {
    if (filterStatus !== "all" && order.status !== filterStatus) {
      return false;
    }
    if (
      searchQuery &&
      !order.item.name.toLowerCase().includes(searchQuery.toLowerCase()) &&
      !order.id.toLowerCase().includes(searchQuery.toLowerCase())
    ) {
      return false;
    }
    return true;
  });

  const stats = {
    total: orders.length,
    inProgress: orders.filter((o) => o.status === "in-progress").length,
    completed: orders.filter((o) => o.status === "completed").length,
    pending: orders.filter((o) => o.status === "pending").length,
  };

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold">Orders</h1>
          <p className="text-muted-foreground">
            Manage your purchases and sales
          </p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-primary/10 p-3">
              <Package className="h-6 w-6 text-primary" />
            </div>
            <div>
              <p className="text-2xl font-bold">{stats.total}</p>
              <p className="text-sm text-muted-foreground">Total Orders</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-blue-500/10 p-3">
              <RefreshCw className="h-6 w-6 text-blue-500" />
            </div>
            <div>
              <p className="text-2xl font-bold">{stats.inProgress}</p>
              <p className="text-sm text-muted-foreground">In Progress</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-green-500/10 p-3">
              <CheckCircle2 className="h-6 w-6 text-green-500" />
            </div>
            <div>
              <p className="text-2xl font-bold">{stats.completed}</p>
              <p className="text-sm text-muted-foreground">Completed</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-amber-500/10 p-3">
              <Clock className="h-6 w-6 text-amber-500" />
            </div>
            <div>
              <p className="text-2xl font-bold">{stats.pending}</p>
              <p className="text-sm text-muted-foreground">Pending</p>
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="all" className="w-full">
        <TabsList>
          <TabsTrigger value="all">All Orders</TabsTrigger>
          <TabsTrigger value="purchases">Purchases</TabsTrigger>
          <TabsTrigger value="sales">Sales</TabsTrigger>
        </TabsList>

        <TabsContent value="all" className="mt-6 space-y-4">
          {/* Filters */}
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="relative flex-1 sm:max-w-md">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search orders..."
                className="pl-9"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
            <div className="flex items-center gap-2">
              <Select value={filterStatus} onValueChange={setFilterStatus}>
                <SelectTrigger className="w-[180px]">
                  <SelectValue placeholder="Filter by status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="pending">Pending</SelectItem>
                  <SelectItem value="in-progress">In Progress</SelectItem>
                  <SelectItem value="delivered">Delivered</SelectItem>
                  <SelectItem value="completed">Completed</SelectItem>
                  <SelectItem value="cancelled">Cancelled</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Orders list */}
          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className="space-y-4"
          >
            {filteredOrders.map((order) => {
              const status = statusConfig[order.status];
              const StatusIcon = status.icon;
              const counterparty = order.type === "purchase" ? order.seller : order.buyer;

              return (
                <motion.div key={order.id} variants={itemVariants}>
                  <Card className="overflow-hidden transition-all hover:shadow-md">
                    <CardContent className="p-0">
                      <div className="flex flex-col lg:flex-row">
                        {/* Order info */}
                        <div className="flex-1 p-6">
                          <div className="flex items-start gap-4">
                            <div className="relative h-20 w-20 shrink-0 overflow-hidden rounded-lg">
                              <Image
                                src={order.item.image}
                                alt={order.item.name}
                                fill
                                className="object-cover"
                              />
                            </div>
                            <div className="flex-1">
                              <div className="flex items-start justify-between">
                                <div>
                                  <Badge
                                    variant="outline"
                                    className={cn("mb-2", status.color)}
                                  >
                                    <StatusIcon className="mr-1 h-3 w-3" />
                                    {status.label}
                                  </Badge>
                                  <h3 className="font-semibold">{order.item.name}</h3>
                                  <p className="text-sm text-muted-foreground">
                                    {order.id} • {order.item.category}
                                  </p>
                                </div>
                                <Badge variant="secondary">
                                  {order.type === "purchase" ? "Purchase" : "Sale"}
                                </Badge>
                              </div>

                              <div className="mt-3 flex items-center gap-3">
                                <Avatar className="h-8 w-8">
                                  <AvatarImage src={counterparty?.avatar} />
                                  <AvatarFallback>
                                    {counterparty?.name[0]}
                                  </AvatarFallback>
                                </Avatar>
                                <div>
                                  <p className="text-sm font-medium">
                                    {counterparty?.name}
                                  </p>
                                  {order.type === "purchase" && order.seller?.rating && (
                                    <div className="flex items-center gap-1 text-xs text-muted-foreground">
                                      <Star className="h-3 w-3 fill-amber-400 text-amber-400" />
                                      {order.seller.rating}
                                    </div>
                                  )}
                                </div>
                              </div>
                            </div>
                          </div>

                          {/* Progress bar for active orders */}
                          {order.progress !== undefined && order.status !== "cancelled" && (
                            <div className="mt-4 space-y-2">
                              <div className="flex items-center justify-between text-sm">
                                <span className="text-muted-foreground">Progress</span>
                                <span className="font-medium">{order.progress}%</span>
                              </div>
                              <Progress value={order.progress} className="h-2" />
                            </div>
                          )}

                          {/* Milestones */}
                          {order.milestones && (
                            <div className="mt-4 flex flex-wrap gap-2">
                              {order.milestones.map((milestone, index) => (
                                <Badge
                                  key={index}
                                  variant="outline"
                                  className={cn(
                                    milestone.completed
                                      ? "bg-green-500/10 text-green-500"
                                      : "bg-gray-500/10 text-gray-500"
                                  )}
                                >
                                  {milestone.completed ? (
                                    <CheckCircle2 className="mr-1 h-3 w-3" />
                                  ) : (
                                    <Clock className="mr-1 h-3 w-3" />
                                  )}
                                  {milestone.name}
                                </Badge>
                              ))}
                            </div>
                          )}

                          {/* Cancellation reason */}
                          {order.cancellationReason && (
                            <div className="mt-4 rounded-lg bg-red-500/10 p-3 text-sm text-red-500">
                              <AlertCircle className="mr-2 inline-block h-4 w-4" />
                              {order.cancellationReason}
                            </div>
                          )}
                        </div>

                        {/* Order sidebar */}
                        <div className="flex flex-col justify-between border-t bg-muted/30 p-6 lg:w-64 lg:border-l lg:border-t-0">
                          <div className="space-y-3">
                            <div>
                              <p className="text-sm text-muted-foreground">Order Total</p>
                              <p className="text-2xl font-bold">
                                {formatCurrency(order.amount)}
                              </p>
                            </div>
                            <div className="flex items-center gap-2 text-sm text-muted-foreground">
                              <Calendar className="h-4 w-4" />
                              <span>Ordered: {order.orderDate}</span>
                            </div>
                            {order.deliveryDate && (
                              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                                <Truck className="h-4 w-4" />
                                <span>
                                  {order.status === "completed"
                                    ? "Delivered"
                                    : "Due"}
                                  : {order.deliveryDate}
                                </span>
                              </div>
                            )}
                          </div>

                          <div className="mt-4 space-y-2">
                            {order.awaitingReview && (
                              <Button className="w-full">
                                <Star className="mr-2 h-4 w-4" />
                                Leave Review
                              </Button>
                            )}
                            {order.status === "in-progress" && (
                              <Button className="w-full">
                                <Eye className="mr-2 h-4 w-4" />
                                View Progress
                              </Button>
                            )}
                            <Button variant="outline" className="w-full">
                              <MessageSquare className="mr-2 h-4 w-4" />
                              Message
                            </Button>
                            <DropdownMenu>
                              <DropdownMenuTrigger asChild>
                                <Button variant="ghost" className="w-full">
                                  <MoreHorizontal className="mr-2 h-4 w-4" />
                                  More Actions
                                </Button>
                              </DropdownMenuTrigger>
                              <DropdownMenuContent align="end">
                                <DropdownMenuItem>
                                  <FileText className="mr-2 h-4 w-4" />
                                  View Invoice
                                </DropdownMenuItem>
                                <DropdownMenuItem>
                                  <ArrowUpRight className="mr-2 h-4 w-4" />
                                  Open Dispute
                                </DropdownMenuItem>
                              </DropdownMenuContent>
                            </DropdownMenu>
                          </div>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              );
            })}
          </motion.div>

          {filteredOrders.length === 0 && (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <Package className="h-12 w-12 text-muted-foreground/50" />
                <h3 className="mt-4 text-lg font-semibold">No orders found</h3>
                <p className="text-muted-foreground">
                  Try adjusting your filters or search terms
                </p>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="purchases" className="mt-6">
          <Card>
            <CardContent className="flex flex-col items-center justify-center py-12">
              <Package className="h-12 w-12 text-muted-foreground/50" />
              <h3 className="mt-4 text-lg font-semibold">Your Purchases</h3>
              <p className="text-muted-foreground">
                View and manage services you&apos;ve purchased
              </p>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="sales" className="mt-6">
          <Card>
            <CardContent className="flex flex-col items-center justify-center py-12">
              <Package className="h-12 w-12 text-muted-foreground/50" />
              <h3 className="mt-4 text-lg font-semibold">Your Sales</h3>
              <p className="text-muted-foreground">
                View and manage services you&apos;ve sold
              </p>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
