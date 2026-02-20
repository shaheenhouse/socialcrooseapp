"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { motion, AnimatePresence } from "framer-motion";
import {
  Tag, Plus, Trash2, Copy, MoreVertical, Percent,
  DollarSign, Calendar, Users, Loader2, CheckCircle2, XCircle,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogFooter,
} from "@/components/ui/dialog";
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from "@/components/ui/select";
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { discountApi } from "@/lib/api";
import { formatCurrency, formatDate } from "@/lib/utils";
import { toast } from "sonner";

interface Discount {
  id: string;
  code: string;
  type: string;
  value: number;
  minOrderAmount?: number;
  maxUses?: number;
  usedCount: number;
  expiresAt?: string;
  isActive: boolean;
  createdAt: string;
}

export default function DiscountsPage() {
  const queryClient = useQueryClient();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [form, setForm] = useState({
    code: "",
    type: "percentage",
    value: 0,
    minOrderAmount: 0,
    maxUses: 0,
    expiresAt: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["discounts"],
    queryFn: () => discountApi.getAll({ pageSize: 50 }),
  });

  const discounts: Discount[] = data?.data?.items ?? data?.data ?? [];

  const createDiscount = useMutation({
    mutationFn: () => discountApi.create({
      ...form,
      value: Number(form.value),
      minOrderAmount: form.minOrderAmount ? Number(form.minOrderAmount) : undefined,
      maxUses: form.maxUses ? Number(form.maxUses) : undefined,
      expiresAt: form.expiresAt || undefined,
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["discounts"] });
      setDialogOpen(false);
      setForm({ code: "", type: "percentage", value: 0, minOrderAmount: 0, maxUses: 0, expiresAt: "" });
    },
  });

  const deleteDiscount = useMutation({
    mutationFn: (id: string) => discountApi.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["discounts"] }),
  });

  const copyCode = (code: string) => {
    navigator.clipboard.writeText(code);
    toast.success("Code copied!");
  };

  const generateCode = () => {
    const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    let code = "";
    for (let i = 0; i < 8; i++) code += chars.charAt(Math.floor(Math.random() * chars.length));
    setForm((f) => ({ ...f, code }));
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <Tag className="h-6 w-6" /> Discount Codes
          </h1>
          <p className="text-muted-foreground text-sm mt-1">
            Manage promotional codes for your store
          </p>
        </div>
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2"><Plus className="h-4 w-4" /> Create Discount</Button>
          </DialogTrigger>
          <DialogContent className="max-w-md">
            <DialogHeader><DialogTitle>Create Discount Code</DialogTitle></DialogHeader>
            <div className="space-y-4 mt-4">
              <div>
                <label className="text-sm font-medium mb-1.5 block">Code</label>
                <div className="flex gap-2">
                  <Input
                    placeholder="SUMMER25"
                    value={form.code}
                    onChange={(e) => setForm((f) => ({ ...f, code: e.target.value.toUpperCase() }))}
                  />
                  <Button variant="outline" size="sm" onClick={generateCode}>Generate</Button>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-sm font-medium mb-1.5 block">Type</label>
                  <Select value={form.type} onValueChange={(v) => setForm((f) => ({ ...f, type: v }))}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="percentage">Percentage</SelectItem>
                      <SelectItem value="fixed">Fixed Amount</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <label className="text-sm font-medium mb-1.5 block">Value</label>
                  <Input
                    type="number"
                    placeholder={form.type === "percentage" ? "25" : "10.00"}
                    value={form.value || ""}
                    onChange={(e) => setForm((f) => ({ ...f, value: parseFloat(e.target.value) || 0 }))}
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-sm font-medium mb-1.5 block">Min Order</label>
                  <Input
                    type="number"
                    placeholder="0"
                    value={form.minOrderAmount || ""}
                    onChange={(e) => setForm((f) => ({ ...f, minOrderAmount: parseFloat(e.target.value) || 0 }))}
                  />
                </div>
                <div>
                  <label className="text-sm font-medium mb-1.5 block">Max Uses</label>
                  <Input
                    type="number"
                    placeholder="Unlimited"
                    value={form.maxUses || ""}
                    onChange={(e) => setForm((f) => ({ ...f, maxUses: parseInt(e.target.value) || 0 }))}
                  />
                </div>
              </div>
              <div>
                <label className="text-sm font-medium mb-1.5 block">Expires At</label>
                <Input
                  type="datetime-local"
                  value={form.expiresAt}
                  onChange={(e) => setForm((f) => ({ ...f, expiresAt: e.target.value }))}
                />
              </div>
            </div>
            <DialogFooter className="mt-4">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancel</Button>
              <Button onClick={() => createDiscount.mutate()} disabled={!form.code || !form.value}>
                Create
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {[
          { label: "Total Codes", value: discounts.length, icon: Tag },
          { label: "Active", value: discounts.filter((d) => d.isActive).length, icon: CheckCircle2 },
          { label: "Expired", value: discounts.filter((d) => d.expiresAt && new Date(d.expiresAt) < new Date()).length, icon: XCircle },
          { label: "Total Uses", value: discounts.reduce((sum, d) => sum + d.usedCount, 0), icon: Users },
        ].map((stat) => (
          <Card key={stat.label}>
            <CardContent className="p-4 flex items-center gap-3">
              <div className="p-2 rounded-lg bg-primary/10">
                <stat.icon className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-2xl font-bold">{stat.value}</p>
                <p className="text-xs text-muted-foreground">{stat.label}</p>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Discount List */}
      {discounts.length === 0 ? (
        <Card className="py-16 text-center">
          <CardContent>
            <Tag className="h-16 w-16 mx-auto mb-4 text-muted-foreground/50" />
            <h3 className="text-lg font-semibold mb-1">No discount codes yet</h3>
            <p className="text-muted-foreground text-sm">Create your first promo code</p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          <AnimatePresence>
            {discounts.map((discount) => {
              const isExpired = discount.expiresAt && new Date(discount.expiresAt) < new Date();
              const isMaxUsed = discount.maxUses && discount.usedCount >= discount.maxUses;

              return (
                <motion.div
                  key={discount.id}
                  layout
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, x: -100 }}
                >
                  <Card className={(!discount.isActive || isExpired) ? "opacity-60" : ""}>
                    <CardContent className="p-4">
                      <div className="flex items-center gap-4">
                        <div className="p-3 rounded-lg bg-primary/10">
                          {discount.type === "percentage"
                            ? <Percent className="h-6 w-6 text-primary" />
                            : <DollarSign className="h-6 w-6 text-primary" />}
                        </div>
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <code className="font-mono font-bold text-lg">{discount.code}</code>
                            <Button variant="ghost" size="icon" className="h-6 w-6" onClick={() => copyCode(discount.code)}>
                              <Copy className="h-3 w-3" />
                            </Button>
                            {discount.isActive && !isExpired && !isMaxUsed && (
                              <Badge variant="default" className="bg-green-500">Active</Badge>
                            )}
                            {isExpired && <Badge variant="destructive">Expired</Badge>}
                            {isMaxUsed && <Badge variant="secondary">Maxed Out</Badge>}
                          </div>
                          <div className="flex items-center gap-4 text-sm text-muted-foreground">
                            <span>
                              {discount.type === "percentage"
                                ? `${discount.value}% off`
                                : `${formatCurrency(discount.value)} off`}
                            </span>
                            {discount.minOrderAmount && discount.minOrderAmount > 0 && (
                              <span>Min: {formatCurrency(discount.minOrderAmount)}</span>
                            )}
                            <span className="flex items-center gap-1">
                              <Users className="h-3 w-3" />
                              {discount.usedCount}{discount.maxUses ? `/${discount.maxUses}` : ""} used
                            </span>
                            {discount.expiresAt && (
                              <span className="flex items-center gap-1">
                                <Calendar className="h-3 w-3" />
                                {formatDate(discount.expiresAt)}
                              </span>
                            )}
                          </div>
                        </div>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon"><MoreVertical className="h-4 w-4" /></Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem onClick={() => copyCode(discount.code)}>
                              <Copy className="h-4 w-4 mr-2" /> Copy code
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              className="text-destructive"
                              onClick={() => deleteDiscount.mutate(discount.id)}
                            >
                              <Trash2 className="h-4 w-4 mr-2" /> Delete
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              );
            })}
          </AnimatePresence>
        </div>
      )}
    </div>
  );
}
