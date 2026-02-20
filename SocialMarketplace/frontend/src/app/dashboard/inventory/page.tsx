'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Plus, Search, Download, Upload, MoreHorizontal, Package,
  AlertTriangle, TrendingUp, BarChart3, Filter, ArrowUpDown,
  Box, Layers, Tag, RefreshCw,
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle,
} from '@/components/ui/dialog';
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { inventoryApi } from '@/lib/api';
import { formatCurrency } from '@/lib/utils';

export default function InventoryPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [showAdjust, setShowAdjust] = useState<string | null>(null);
  const [adjustData, setAdjustData] = useState({ quantity: 0, type: 'add' as 'add' | 'remove' | 'set', reason: '' });

  const { data: inventoryData, isLoading } = useQuery({
    queryKey: ['inventory', search],
    queryFn: () => inventoryApi.getAll({ search: search || undefined }),
  });

  const { data: lowStockData } = useQuery({
    queryKey: ['inventory-low-stock'],
    queryFn: () => inventoryApi.getLowStock(),
  });

  const { data: valuationData } = useQuery({
    queryKey: ['inventory-valuation'],
    queryFn: () => inventoryApi.getValuation(),
  });

  const adjustStock = useMutation({
    mutationFn: ({ id, data }: { id: string; data: typeof adjustData }) =>
      inventoryApi.adjustStock(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['inventory'] });
      setShowAdjust(null);
      setAdjustData({ quantity: 0, type: 'add', reason: '' });
    },
  });

  const items = inventoryData?.data?.items ?? inventoryData?.data ?? [];
  const lowStockCount = lowStockData?.data?.length ?? 0;
  const valuation = valuationData?.data;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Inventory</h1>
          <p className="text-muted-foreground">Track and manage your stock levels</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" className="gap-1.5">
            <Upload className="h-4 w-4" /> Import
          </Button>
          <Button variant="outline" size="sm" className="gap-1.5">
            <Download className="h-4 w-4" /> Export
          </Button>
          <Button size="sm" className="gap-1.5">
            <Plus className="h-4 w-4" /> Add Item
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-4">
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 rounded-lg bg-primary/10 flex items-center justify-center">
                <Package className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total Items</p>
                <p className="text-xl font-bold">{valuation?.totalItems ?? items.length}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-4">
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 rounded-lg bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
                <TrendingUp className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total Value</p>
                <p className="text-xl font-bold text-green-600">{formatCurrency(valuation?.totalValue ?? 0)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-4">
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 rounded-lg bg-yellow-100 dark:bg-yellow-900/30 flex items-center justify-center">
                <AlertTriangle className="h-5 w-5 text-yellow-600" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Low Stock</p>
                <p className="text-xl font-bold text-yellow-600">{lowStockCount}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-4">
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
                <Layers className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Categories</p>
                <p className="text-xl font-bold">{valuation?.totalCategories ?? 0}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Search */}
      <div className="flex gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input placeholder="Search inventory..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-10" />
        </div>
        <Button variant="outline" size="icon">
          <Filter className="h-4 w-4" />
        </Button>
        <Button variant="outline" size="icon">
          <ArrowUpDown className="h-4 w-4" />
        </Button>
      </div>

      {/* Inventory List */}
      <div className="space-y-3">
        {isLoading ? (
          Array.from({ length: 6 }).map((_, i) => (
            <Card key={i} className="animate-pulse">
              <CardContent className="py-4">
                <div className="flex items-center gap-4">
                  <div className="h-12 w-12 rounded-lg bg-muted" />
                  <div className="flex-1 space-y-2">
                    <div className="h-4 w-40 bg-muted rounded" />
                    <div className="h-3 w-28 bg-muted rounded" />
                  </div>
                  <div className="h-6 w-20 bg-muted rounded" />
                </div>
              </CardContent>
            </Card>
          ))
        ) : items.length === 0 ? (
          <Card className="py-16 text-center">
            <CardContent>
              <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-muted flex items-center justify-center">
                <Box className="h-8 w-8 text-muted-foreground" />
              </div>
              <h3 className="text-lg font-semibold mb-1">No inventory items</h3>
              <p className="text-muted-foreground text-sm mb-4">
                Add your first item to start tracking inventory.
              </p>
              <Button className="gap-1.5">
                <Plus className="h-4 w-4" /> Add First Item
              </Button>
            </CardContent>
          </Card>
        ) : (
          <AnimatePresence>
            {items.map((item: any, index: number) => {
              const stockPercent = item.reorderLevel > 0
                ? Math.min(100, (item.quantity / (item.reorderLevel * 3)) * 100)
                : 100;
              const isLow = item.quantity <= (item.reorderLevel || 0);

              return (
                <motion.div
                  key={item.id}
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.03 }}
                >
                  <Card className="hover:shadow-sm transition-shadow">
                    <CardContent className="py-4">
                      <div className="flex items-center gap-4">
                        <div className="h-12 w-12 rounded-lg bg-muted flex items-center justify-center">
                          {item.imageUrl ? (
                            <img src={item.imageUrl} alt="" className="h-12 w-12 rounded-lg object-cover" />
                          ) : (
                            <Package className="h-6 w-6 text-muted-foreground" />
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2">
                            <span className="font-medium truncate">{item.name}</span>
                            {item.sku && <span className="text-xs text-muted-foreground">SKU: {item.sku}</span>}
                            {isLow && <Badge variant="destructive" className="text-xs">Low Stock</Badge>}
                          </div>
                          <div className="flex items-center gap-3 mt-1.5">
                            <div className="flex-1 max-w-32">
                              <Progress value={stockPercent} className="h-1.5" />
                            </div>
                            <span className="text-sm font-medium">{item.quantity} {item.unit || 'units'}</span>
                            {item.category && (
                              <Badge variant="outline" className="text-xs">
                                <Tag className="h-3 w-3 mr-1" />{item.category}
                              </Badge>
                            )}
                          </div>
                        </div>
                        <div className="text-right">
                          <div className="font-semibold">{formatCurrency(item.unitPrice ?? 0)}</div>
                          <div className="text-xs text-muted-foreground">per {item.unit || 'unit'}</div>
                        </div>
                        <div className="flex gap-1">
                          <Button
                            variant="outline"
                            size="sm"
                            className="gap-1"
                            onClick={() => {
                              setShowAdjust(item.id);
                              setAdjustData({ quantity: 0, type: 'add', reason: '' });
                            }}
                          >
                            <RefreshCw className="h-3.5 w-3.5" /> Adjust
                          </Button>
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" size="icon" className="h-8 w-8">
                                <MoreHorizontal className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuItem>Edit</DropdownMenuItem>
                              <DropdownMenuItem>View movements</DropdownMenuItem>
                              <DropdownMenuItem className="text-destructive">Delete</DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              );
            })}
          </AnimatePresence>
        )}
      </div>

      {/* Stock Adjustment Dialog */}
      <Dialog open={!!showAdjust} onOpenChange={() => setShowAdjust(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Adjust Stock</DialogTitle>
            <DialogDescription>Update the stock level for this item.</DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="flex gap-2">
              {(['add', 'remove', 'set'] as const).map((type) => (
                <Button
                  key={type}
                  variant={adjustData.type === type ? 'default' : 'outline'}
                  className="flex-1 capitalize"
                  onClick={() => setAdjustData({ ...adjustData, type })}
                >
                  {type === 'add' ? '+ Add' : type === 'remove' ? '- Remove' : '= Set'}
                </Button>
              ))}
            </div>
            <Input
              type="number"
              placeholder="Quantity"
              value={adjustData.quantity || ''}
              onChange={(e) => setAdjustData({ ...adjustData, quantity: parseInt(e.target.value) || 0 })}
            />
            <Input
              placeholder="Reason for adjustment"
              value={adjustData.reason}
              onChange={(e) => setAdjustData({ ...adjustData, reason: e.target.value })}
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowAdjust(null)}>Cancel</Button>
            <Button
              onClick={() => showAdjust && adjustStock.mutate({ id: showAdjust, data: adjustData })}
              disabled={!adjustData.quantity || !adjustData.reason.trim() || adjustStock.isPending}
            >
              {adjustStock.isPending ? 'Adjusting...' : 'Adjust Stock'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
