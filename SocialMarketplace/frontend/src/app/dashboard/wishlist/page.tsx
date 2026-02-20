"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { motion, AnimatePresence } from "framer-motion";
import {
  Heart, Plus, Trash2, ShoppingCart, MoreVertical,
  Package, Star, Loader2, HeartOff,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger,
} from "@/components/ui/dialog";
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { wishlistApi, cartApi } from "@/lib/api";
import { formatCurrency } from "@/lib/utils";

interface WishlistItem {
  id: string;
  productId?: string;
  serviceId?: string;
  productName?: string;
  productImageUrl?: string;
  price?: number;
  storeName?: string;
  addedAt: string;
}

interface Wishlist {
  id: string;
  name: string;
  items: WishlistItem[];
  createdAt: string;
}

export default function WishlistPage() {
  const queryClient = useQueryClient();
  const [newListName, setNewListName] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ["wishlists"],
    queryFn: () => wishlistApi.getAll(),
  });

  const wishlists: Wishlist[] = data?.data ?? [];

  const createWishlist = useMutation({
    mutationFn: (name: string) => wishlistApi.create(name),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["wishlists"] });
      setNewListName("");
      setDialogOpen(false);
    },
  });

  const removeItem = useMutation({
    mutationFn: ({ wishlistId, itemId }: { wishlistId: string; itemId: string }) =>
      wishlistApi.removeItem(wishlistId, itemId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["wishlists"] }),
  });

  const deleteWishlist = useMutation({
    mutationFn: (id: string) => wishlistApi.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["wishlists"] }),
  });

  const addToCart = useMutation({
    mutationFn: (productId: string) => cartApi.addItem({ productId, quantity: 1 }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["cart"] }),
  });

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
            <Heart className="h-6 w-6 text-red-500" /> Wishlists
          </h1>
          <p className="text-muted-foreground text-sm mt-1">
            {wishlists.length} {wishlists.length === 1 ? "list" : "lists"}
          </p>
        </div>
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2"><Plus className="h-4 w-4" /> New List</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader><DialogTitle>Create Wishlist</DialogTitle></DialogHeader>
            <div className="flex gap-2 mt-4">
              <Input
                placeholder="Wishlist name"
                value={newListName}
                onChange={(e) => setNewListName(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter" && newListName.trim()) createWishlist.mutate(newListName);
                }}
              />
              <Button onClick={() => createWishlist.mutate(newListName)} disabled={!newListName.trim()}>
                Create
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      {wishlists.length === 0 ? (
        <Card className="py-16 text-center">
          <CardContent>
            <HeartOff className="h-16 w-16 mx-auto mb-4 text-muted-foreground/50" />
            <h3 className="text-lg font-semibold mb-1">No wishlists yet</h3>
            <p className="text-muted-foreground text-sm mb-4">
              Create a wishlist and save your favorite items
            </p>
            <Button onClick={() => setDialogOpen(true)}>
              <Plus className="h-4 w-4 mr-2" /> Create your first wishlist
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-6">
          {wishlists.map((wishlist) => (
            <Card key={wishlist.id}>
              <CardHeader className="flex-row items-center justify-between space-y-0">
                <CardTitle className="flex items-center gap-2">
                  <Heart className="h-5 w-5 text-red-500 fill-red-500" />
                  {wishlist.name}
                  <Badge variant="secondary">{wishlist.items?.length ?? 0}</Badge>
                </CardTitle>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="icon"><MoreVertical className="h-4 w-4" /></Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem
                      className="text-destructive"
                      onClick={() => deleteWishlist.mutate(wishlist.id)}
                    >
                      <Trash2 className="h-4 w-4 mr-2" /> Delete list
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </CardHeader>
              <CardContent>
                {(!wishlist.items || wishlist.items.length === 0) ? (
                  <p className="text-sm text-muted-foreground py-4 text-center">
                    No items in this wishlist
                  </p>
                ) : (
                  <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-3">
                    <AnimatePresence>
                      {wishlist.items.map((item) => (
                        <motion.div
                          key={item.id}
                          layout
                          initial={{ opacity: 0, scale: 0.95 }}
                          animate={{ opacity: 1, scale: 1 }}
                          exit={{ opacity: 0, scale: 0.95 }}
                        >
                          <Card className="overflow-hidden">
                            <div className="aspect-square bg-muted flex items-center justify-center">
                              {item.productImageUrl ? (
                                <img src={item.productImageUrl} alt={item.productName ?? ""} className="w-full h-full object-cover" />
                              ) : (
                                <Package className="h-10 w-10 text-muted-foreground" />
                              )}
                            </div>
                            <CardContent className="p-3">
                              <h4 className="font-medium text-sm truncate">{item.productName ?? "Item"}</h4>
                              {item.storeName && (
                                <p className="text-xs text-muted-foreground">{item.storeName}</p>
                              )}
                              {item.price && (
                                <p className="font-semibold mt-1">{formatCurrency(item.price)}</p>
                              )}
                              <div className="flex gap-2 mt-2">
                                {item.productId && (
                                  <Button
                                    size="sm" variant="default" className="flex-1 gap-1"
                                    onClick={() => addToCart.mutate(item.productId!)}
                                  >
                                    <ShoppingCart className="h-3 w-3" /> Add to Cart
                                  </Button>
                                )}
                                <Button
                                  size="sm" variant="ghost" className="text-destructive"
                                  onClick={() => removeItem.mutate({ wishlistId: wishlist.id, itemId: item.id })}
                                >
                                  <Trash2 className="h-3 w-3" />
                                </Button>
                              </div>
                            </CardContent>
                          </Card>
                        </motion.div>
                      ))}
                    </AnimatePresence>
                  </div>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
