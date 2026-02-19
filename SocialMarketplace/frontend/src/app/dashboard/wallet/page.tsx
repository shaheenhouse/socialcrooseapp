"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import {
  Wallet,
  ArrowUpRight,
  ArrowDownLeft,
  CreditCard,
  Building2,
  DollarSign,
  TrendingUp,
  TrendingDown,
  Clock,
  CheckCircle2,
  XCircle,
  AlertCircle,
  Plus,
  Send,
  Download,
  Filter,
  Calendar,
  Eye,
  EyeOff,
  RefreshCw,
  Shield,
  Smartphone,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Progress } from "@/components/ui/progress";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

const walletData = {
  balance: 125430.50,
  pendingBalance: 45000.00,
  escrowBalance: 85000.00,
  currency: "PKR",
  monthlyEarnings: 234500,
  monthlySpending: 45000,
};

const transactions = [
  {
    id: "tx1",
    type: "credit",
    description: "Payment received from Tech Corp",
    project: "E-commerce Platform",
    amount: 45000,
    status: "completed",
    date: "Dec 1, 2025",
    time: "2:30 PM",
    reference: "PAY-2025-78234",
  },
  {
    id: "tx2",
    type: "debit",
    description: "Service fee",
    project: "Platform Fee",
    amount: 2250,
    status: "completed",
    date: "Dec 1, 2025",
    time: "2:30 PM",
    reference: "FEE-2025-78234",
  },
  {
    id: "tx3",
    type: "credit",
    description: "Milestone payment - Healthcare App",
    project: "MediCare Project",
    amount: 35000,
    status: "completed",
    date: "Nov 28, 2025",
    time: "11:15 AM",
    reference: "PAY-2025-77456",
  },
  {
    id: "tx4",
    type: "withdrawal",
    description: "Bank withdrawal",
    project: "Allied Bank ****4532",
    amount: 100000,
    status: "pending",
    date: "Nov 27, 2025",
    time: "4:00 PM",
    reference: "WTH-2025-76890",
  },
  {
    id: "tx5",
    type: "credit",
    description: "Product sale - Digital Marketing Guide",
    project: "Marketplace Sale",
    amount: 5000,
    status: "completed",
    date: "Nov 25, 2025",
    time: "9:45 AM",
    reference: "SAL-2025-75234",
  },
  {
    id: "tx6",
    type: "escrow",
    description: "Escrow released - Web Development",
    project: "DataDriven Co",
    amount: 65000,
    status: "completed",
    date: "Nov 23, 2025",
    time: "3:20 PM",
    reference: "ESC-2025-74567",
  },
];

const paymentMethods = [
  {
    id: "pm1",
    type: "bank",
    name: "Allied Bank",
    details: "****4532",
    isPrimary: true,
    icon: Building2,
  },
  {
    id: "pm2",
    type: "easypaisa",
    name: "Easypaisa",
    details: "0300-****890",
    isPrimary: false,
    icon: Smartphone,
  },
  {
    id: "pm3",
    type: "jazzcash",
    name: "JazzCash",
    details: "0321-****567",
    isPrimary: false,
    icon: Smartphone,
  },
];

const escrowItems = [
  {
    id: "esc1",
    project: "E-commerce Platform",
    client: "Tech Solutions Inc",
    amount: 45000,
    releaseDate: "Dec 15, 2025",
    milestone: "Phase 2 Completion",
    progress: 75,
  },
  {
    id: "esc2",
    project: "Healthcare Dashboard",
    client: "MediCare Plus",
    amount: 40000,
    releaseDate: "Jan 10, 2026",
    milestone: "Final Delivery",
    progress: 60,
  },
];

const statusColors: Record<string, string> = {
  completed: "bg-green-500/10 text-green-500",
  pending: "bg-amber-500/10 text-amber-500",
  failed: "bg-red-500/10 text-red-500",
};

const typeIcons: Record<string, React.ElementType> = {
  credit: ArrowDownLeft,
  debit: ArrowUpRight,
  withdrawal: ArrowUpRight,
  escrow: Shield,
};

const typeColors: Record<string, string> = {
  credit: "text-green-500",
  debit: "text-red-500",
  withdrawal: "text-orange-500",
  escrow: "text-blue-500",
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
    maximumFractionDigits: 0,
  }).format(amount);
}

export default function WalletPage() {
  const [showBalance, setShowBalance] = useState(true);
  const [filterPeriod, setFilterPeriod] = useState("all");

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold">Wallet</h1>
          <p className="text-muted-foreground">
            Manage your earnings, payments, and withdrawals
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline">
            <Download className="mr-2 h-4 w-4" />
            Export
          </Button>
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Add Funds
          </Button>
        </div>
      </div>

      {/* Balance cards */}
      <div className="grid gap-4 md:grid-cols-3">
        {/* Main balance */}
        <Card className="md:col-span-2">
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Available Balance</p>
                <div className="mt-1 flex items-center gap-2">
                  <span className="text-4xl font-bold">
                    {showBalance
                      ? formatCurrency(walletData.balance)
                      : "****"}
                  </span>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setShowBalance(!showBalance)}
                  >
                    {showBalance ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </Button>
                </div>
              </div>
              <div className="rounded-full bg-primary/10 p-3">
                <Wallet className="h-8 w-8 text-primary" />
              </div>
            </div>

            <div className="mt-6 grid gap-4 sm:grid-cols-2">
              <div className="rounded-lg bg-amber-500/10 p-4">
                <div className="flex items-center gap-2">
                  <Clock className="h-4 w-4 text-amber-500" />
                  <span className="text-sm text-muted-foreground">Pending</span>
                </div>
                <p className="mt-1 text-xl font-semibold">
                  {formatCurrency(walletData.pendingBalance)}
                </p>
              </div>
              <div className="rounded-lg bg-blue-500/10 p-4">
                <div className="flex items-center gap-2">
                  <Shield className="h-4 w-4 text-blue-500" />
                  <span className="text-sm text-muted-foreground">In Escrow</span>
                </div>
                <p className="mt-1 text-xl font-semibold">
                  {formatCurrency(walletData.escrowBalance)}
                </p>
              </div>
            </div>

            <div className="mt-6 flex gap-2">
              <Button className="flex-1">
                <Send className="mr-2 h-4 w-4" />
                Withdraw
              </Button>
              <Button variant="outline" className="flex-1">
                <RefreshCw className="mr-2 h-4 w-4" />
                Transfer
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Quick stats */}
        <div className="space-y-4">
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="rounded-full bg-green-500/10 p-2">
                  <TrendingUp className="h-5 w-5 text-green-500" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">This Month Earnings</p>
                  <p className="text-xl font-bold">
                    {formatCurrency(walletData.monthlyEarnings)}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="rounded-full bg-red-500/10 p-2">
                  <TrendingDown className="h-5 w-5 text-red-500" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">This Month Spending</p>
                  <p className="text-xl font-bold">
                    {formatCurrency(walletData.monthlySpending)}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      <Tabs defaultValue="transactions" className="w-full">
        <TabsList>
          <TabsTrigger value="transactions">Transactions</TabsTrigger>
          <TabsTrigger value="escrow">Escrow</TabsTrigger>
          <TabsTrigger value="payment-methods">Payment Methods</TabsTrigger>
        </TabsList>

        {/* Transactions Tab */}
        <TabsContent value="transactions" className="mt-6 space-y-4">
          {/* Filters */}
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-2">
              <Select value={filterPeriod} onValueChange={setFilterPeriod}>
                <SelectTrigger className="w-[180px]">
                  <Calendar className="mr-2 h-4 w-4" />
                  <SelectValue placeholder="Period" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Time</SelectItem>
                  <SelectItem value="today">Today</SelectItem>
                  <SelectItem value="week">This Week</SelectItem>
                  <SelectItem value="month">This Month</SelectItem>
                  <SelectItem value="year">This Year</SelectItem>
                </SelectContent>
              </Select>
              <Button variant="outline" size="icon">
                <Filter className="h-4 w-4" />
              </Button>
            </div>
          </div>

          {/* Transactions list */}
          <Card>
            <CardContent className="p-0">
              <motion.div
                variants={containerVariants}
                initial="hidden"
                animate="visible"
              >
                {transactions.map((tx, index) => {
                  const Icon = typeIcons[tx.type];
                  return (
                    <motion.div
                      key={tx.id}
                      variants={itemVariants}
                      className={cn(
                        "flex items-center justify-between p-4",
                        index !== transactions.length - 1 && "border-b"
                      )}
                    >
                      <div className="flex items-center gap-4">
                        <div
                          className={cn(
                            "rounded-full p-2",
                            tx.type === "credit"
                              ? "bg-green-500/10"
                              : tx.type === "debit" || tx.type === "withdrawal"
                              ? "bg-red-500/10"
                              : "bg-blue-500/10"
                          )}
                        >
                          <Icon
                            className={cn("h-5 w-5", typeColors[tx.type])}
                          />
                        </div>
                        <div>
                          <p className="font-medium">{tx.description}</p>
                          <p className="text-sm text-muted-foreground">
                            {tx.project} • {tx.reference}
                          </p>
                        </div>
                      </div>
                      <div className="flex items-center gap-4">
                        <div className="text-right">
                          <p
                            className={cn(
                              "font-semibold",
                              tx.type === "credit" || tx.type === "escrow"
                                ? "text-green-500"
                                : "text-red-500"
                            )}
                          >
                            {tx.type === "credit" || tx.type === "escrow"
                              ? "+"
                              : "-"}
                            {formatCurrency(tx.amount)}
                          </p>
                          <p className="text-xs text-muted-foreground">
                            {tx.date} • {tx.time}
                          </p>
                        </div>
                        <Badge
                          variant="outline"
                          className={statusColors[tx.status]}
                        >
                          {tx.status === "completed" ? (
                            <CheckCircle2 className="mr-1 h-3 w-3" />
                          ) : tx.status === "pending" ? (
                            <Clock className="mr-1 h-3 w-3" />
                          ) : (
                            <XCircle className="mr-1 h-3 w-3" />
                          )}
                          {tx.status}
                        </Badge>
                      </div>
                    </motion.div>
                  );
                })}
              </motion.div>
            </CardContent>
            <CardFooter className="justify-center border-t p-4">
              <Button variant="ghost">Load More Transactions</Button>
            </CardFooter>
          </Card>
        </TabsContent>

        {/* Escrow Tab */}
        <TabsContent value="escrow" className="mt-6 space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Active Escrow</CardTitle>
              <CardDescription>
                Funds held securely until milestone completion
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {escrowItems.map((item) => (
                <div
                  key={item.id}
                  className="rounded-lg border p-4"
                >
                  <div className="flex items-start justify-between">
                    <div>
                      <h4 className="font-semibold">{item.project}</h4>
                      <p className="text-sm text-muted-foreground">
                        {item.client} • {item.milestone}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="text-xl font-bold text-primary">
                        {formatCurrency(item.amount)}
                      </p>
                      <p className="text-sm text-muted-foreground">
                        Release: {item.releaseDate}
                      </p>
                    </div>
                  </div>
                  <div className="mt-4 space-y-2">
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">
                        Project Progress
                      </span>
                      <span className="font-medium">{item.progress}%</span>
                    </div>
                    <Progress value={item.progress} className="h-2" />
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>

          <Card>
            <CardContent className="flex items-center gap-4 p-6">
              <div className="rounded-full bg-blue-500/10 p-3">
                <Shield className="h-6 w-6 text-blue-500" />
              </div>
              <div className="flex-1">
                <h4 className="font-semibold">Escrow Protection</h4>
                <p className="text-sm text-muted-foreground">
                  Your funds are protected until you approve the work delivery
                </p>
              </div>
              <Button variant="outline">Learn More</Button>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Payment Methods Tab */}
        <TabsContent value="payment-methods" className="mt-6 space-y-4">
          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className="grid gap-4 md:grid-cols-2 lg:grid-cols-3"
          >
            {paymentMethods.map((method) => (
              <motion.div key={method.id} variants={itemVariants}>
                <Card
                  className={cn(
                    "relative transition-all hover:shadow-md",
                    method.isPrimary && "border-primary"
                  )}
                >
                  {method.isPrimary && (
                    <Badge className="absolute -top-2 right-4">Primary</Badge>
                  )}
                  <CardContent className="p-6">
                    <div className="flex items-center gap-4">
                      <div className="rounded-full bg-muted p-3">
                        <method.icon className="h-6 w-6" />
                      </div>
                      <div>
                        <p className="font-semibold">{method.name}</p>
                        <p className="text-sm text-muted-foreground">
                          {method.details}
                        </p>
                      </div>
                    </div>
                    <div className="mt-4 flex gap-2">
                      <Button variant="outline" size="sm" className="flex-1">
                        Edit
                      </Button>
                      {!method.isPrimary && (
                        <Button size="sm" className="flex-1">
                          Set Primary
                        </Button>
                      )}
                    </div>
                  </CardContent>
                </Card>
              </motion.div>
            ))}

            {/* Add new method */}
            <motion.div variants={itemVariants}>
              <Card className="flex h-full min-h-[160px] cursor-pointer items-center justify-center border-dashed transition-all hover:border-primary hover:bg-muted/50">
                <div className="text-center">
                  <Plus className="mx-auto h-8 w-8 text-muted-foreground" />
                  <p className="mt-2 font-medium">Add Payment Method</p>
                </div>
              </Card>
            </motion.div>
          </motion.div>

          {/* Supported methods */}
          <Card>
            <CardHeader>
              <CardTitle>Supported Payment Methods</CardTitle>
              <CardDescription>
                We support various payment options for your convenience
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
                <div className="flex items-center gap-2 rounded-lg border p-3">
                  <Building2 className="h-5 w-5" />
                  <span className="text-sm">Bank Transfer</span>
                </div>
                <div className="flex items-center gap-2 rounded-lg border p-3">
                  <Smartphone className="h-5 w-5" />
                  <span className="text-sm">Easypaisa</span>
                </div>
                <div className="flex items-center gap-2 rounded-lg border p-3">
                  <Smartphone className="h-5 w-5" />
                  <span className="text-sm">JazzCash</span>
                </div>
                <div className="flex items-center gap-2 rounded-lg border p-3">
                  <CreditCard className="h-5 w-5" />
                  <span className="text-sm">Debit Card</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
