"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { motion, AnimatePresence } from "framer-motion";
import {
  ShoppingCart, Minus, Plus, Trash2, ArrowRight,
  Package, Tag, Loader2, ShoppingBag,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import { cartApi, discountApi } from "@/lib/api";
import { formatCurrency } from "@/lib/utils";
import Link from "next/link";

interface CartItem {
  id: string;
  productId: string;
  productName: string;
  productImageUrl?: string;
  price: number;
  quantity: number;
  storeName?: string;
}

export default function CartPage() {
  const queryClient = useQueryClient();
  const [discountCode, setDiscountCode] = useState("");
  const [discountResult, setDiscountResult] = useState<{ valid: boolean; discount: number; message: string } | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["cart"],
    queryFn: () => cartApi.get(),
  });

  const items: CartItem[] = data?.data?.items ?? [];

  const updateQuantity = useMutation({
    mutationFn: ({ itemId, quantity }: { itemId: string; quantity: number }) =>
      cartApi.updateItem(itemId, quantity),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["cart"] }),
  });

  const removeItem = useMutation({
    mutationFn: (itemId: string) => cartApi.removeItem(itemId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["cart"] }),
  });

  const clearCart = useMutation({
    mutationFn: () => cartApi.clear(),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["cart"] }),
  });

  const validateDiscount = useMutation({
    mutationFn: () => discountApi.validate(discountCode, subtotal),
    onSuccess: (res) => setDiscountResult(res.data),
    onError: () => setDiscountResult({ valid: false, discount: 0, message: "Invalid code" }),
  });

  const subtotal = items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  const discount = discountResult?.valid ? discountResult.discount : 0;
  const total = subtotal - discount;

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
            <ShoppingCart className="h-6 w-6" /> Shopping Cart
          </h1>
          <p className="text-muted-foreground text-sm mt-1">
            {items.length} {items.length === 1 ? "item" : "items"} in your cart
          </p>
        </div>
        {items.length > 0 && (
          <Button variant="ghost" size="sm" className="text-destructive" onClick={() => clearCart.mutate()}>
            <Trash2 className="h-4 w-4 mr-1" /> Clear all
          </Button>
        )}
      </div>

      {items.length === 0 ? (
        <Card className="py-16 text-center">
          <CardContent>
            <ShoppingBag className="h-16 w-16 mx-auto mb-4 text-muted-foreground/50" />
            <h3 className="text-lg font-semibold mb-1">Your cart is empty</h3>
            <p className="text-muted-foreground text-sm mb-4">
              Browse the marketplace and add items to your cart
            </p>
            <Button asChild>
              <Link href="/dashboard/marketplace">Browse Marketplace</Link>
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid lg:grid-cols-3 gap-6">
          {/* Cart Items */}
          <div className="lg:col-span-2 space-y-3">
            <AnimatePresence>
              {items.map((item) => (
                <motion.div
                  key={item.id}
                  layout
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, x: -100 }}
                >
                  <Card>
                    <CardContent className="p-4">
                      <div className="flex gap-4">
                        <div className="w-20 h-20 rounded-lg bg-muted flex items-center justify-center overflow-hidden flex-shrink-0">
                          {item.productImageUrl ? (
                            <img src={item.productImageUrl} alt={item.productName} className="w-full h-full object-cover" />
                          ) : (
                            <Package className="h-8 w-8 text-muted-foreground" />
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <h3 className="font-medium truncate">{item.productName}</h3>
                          {item.storeName && (
                            <p className="text-xs text-muted-foreground">{item.storeName}</p>
                          )}
                          <p className="text-lg font-semibold mt-1">{formatCurrency(item.price)}</p>
                        </div>
                        <div className="flex flex-col items-end justify-between">
                          <Button
                            variant="ghost" size="icon" className="h-8 w-8 text-destructive"
                            onClick={() => removeItem.mutate(item.id)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                          <div className="flex items-center gap-2 border rounded-lg">
                            <Button
                              variant="ghost" size="icon" className="h-8 w-8"
                              onClick={() => updateQuantity.mutate({ itemId: item.id, quantity: Math.max(1, item.quantity - 1) })}
                              disabled={item.quantity <= 1}
                            >
                              <Minus className="h-3 w-3" />
                            </Button>
                            <span className="w-8 text-center text-sm font-medium">{item.quantity}</span>
                            <Button
                              variant="ghost" size="icon" className="h-8 w-8"
                              onClick={() => updateQuantity.mutate({ itemId: item.id, quantity: item.quantity + 1 })}
                            >
                              <Plus className="h-3 w-3" />
                            </Button>
                          </div>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              ))}
            </AnimatePresence>
          </div>

          {/* Order Summary */}
          <div>
            <Card className="sticky top-20">
              <CardHeader><CardTitle>Order Summary</CardTitle></CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center gap-2">
                  <Input
                    placeholder="Discount code"
                    value={discountCode}
                    onChange={(e) => setDiscountCode(e.target.value)}
                    className="flex-1"
                  />
                  <Button
                    variant="outline" size="sm"
                    onClick={() => validateDiscount.mutate()}
                    disabled={!discountCode.trim()}
                  >
                    <Tag className="h-4 w-4 mr-1" /> Apply
                  </Button>
                </div>
                {discountResult && (
                  <p className={`text-xs ${discountResult.valid ? "text-green-600" : "text-destructive"}`}>
                    {discountResult.message}
                  </p>
                )}

                <Separator />

                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Subtotal</span>
                    <span>{formatCurrency(subtotal)}</span>
                  </div>
                  {discount > 0 && (
                    <div className="flex justify-between text-green-600">
                      <span>Discount</span>
                      <span>-{formatCurrency(discount)}</span>
                    </div>
                  )}
                  <Separator />
                  <div className="flex justify-between font-semibold text-base">
                    <span>Total</span>
                    <span>{formatCurrency(total)}</span>
                  </div>
                </div>

                <Button className="w-full gap-2" size="lg">
                  Proceed to Checkout <ArrowRight className="h-4 w-4" />
                </Button>
              </CardContent>
            </Card>
          </div>
        </div>
      )}
    </div>
  );
}
