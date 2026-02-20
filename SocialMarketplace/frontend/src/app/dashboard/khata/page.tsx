'use client';

import { useState, useCallback } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Plus, Search, ArrowUpRight, ArrowDownLeft, Wallet, Phone,
  Download, Send, MoreHorizontal, TrendingUp, TrendingDown,
  Users, Receipt, Filter, Calendar,
} from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger, DialogFooter,
} from '@/components/ui/dialog';
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { khataApi } from '@/lib/api';
import { formatCurrency, formatRelativeTime } from '@/lib/utils';

export default function KhataPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [activeTab, setActiveTab] = useState('all');
  const [showAddParty, setShowAddParty] = useState(false);
  const [showAddEntry, setShowAddEntry] = useState<string | null>(null);
  const [newParty, setNewParty] = useState<{ partyName: string; partyPhone: string; type: 'customer' | 'supplier'; openingBalance: number }>({ partyName: '', partyPhone: '', type: 'customer', openingBalance: 0 });
  const [newEntry, setNewEntry] = useState({ amount: 0, type: 'credit' as 'credit' | 'debit', description: '' });

  const { data: partiesData, isLoading } = useQuery({
    queryKey: ['khata', search, activeTab],
    queryFn: () => khataApi.getAll({ search: search || undefined, status: activeTab !== 'all' ? activeTab : undefined }),
  });

  const { data: summaryData } = useQuery({
    queryKey: ['khata-summary'],
    queryFn: () => khataApi.getSummary(),
  });

  const createParty = useMutation({
    mutationFn: (data: typeof newParty) => khataApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['khata'] });
      setShowAddParty(false);
      setNewParty({ partyName: '', partyPhone: '', type: 'customer', openingBalance: 0 });
    },
  });

  const addEntry = useMutation({
    mutationFn: ({ partyId, data }: { partyId: string; data: typeof newEntry }) =>
      khataApi.addEntry(partyId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['khata'] });
      setShowAddEntry(null);
      setNewEntry({ amount: 0, type: 'credit', description: '' });
    },
  });

  const sendReminder = useMutation({
    mutationFn: (partyId: string) => khataApi.sendReminder(partyId),
  });

  const parties = partiesData?.data?.items ?? partiesData?.data ?? [];
  const summary = summaryData?.data;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Khata / Ledger</h1>
          <p className="text-muted-foreground">Track credit and debit with your customers and suppliers</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" className="gap-1.5">
            <Download className="h-4 w-4" /> Export
          </Button>
          <Dialog open={showAddParty} onOpenChange={setShowAddParty}>
            <DialogTrigger asChild>
              <Button size="sm" className="gap-1.5">
                <Plus className="h-4 w-4" /> Add Party
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Add New Party</DialogTitle>
                <DialogDescription>Add a customer or supplier to start tracking their khata.</DialogDescription>
              </DialogHeader>
              <div className="space-y-4 py-4">
                <Input
                  placeholder="Party name"
                  value={newParty.partyName}
                  onChange={(e) => setNewParty({ ...newParty, partyName: e.target.value })}
                />
                <Input
                  placeholder="Phone number (optional)"
                  value={newParty.partyPhone}
                  onChange={(e) => setNewParty({ ...newParty, partyPhone: e.target.value })}
                />
                <div className="flex gap-2">
                  <Button
                    variant={newParty.type === 'customer' ? 'default' : 'outline'}
                    className="flex-1"
                    onClick={() => setNewParty({ ...newParty, type: 'customer' })}
                  >
                    Customer
                  </Button>
                  <Button
                    variant={newParty.type === 'supplier' ? 'default' : 'outline'}
                    className="flex-1"
                    onClick={() => setNewParty({ ...newParty, type: 'supplier' })}
                  >
                    Supplier
                  </Button>
                </div>
                <Input
                  type="number"
                  placeholder="Opening balance (optional)"
                  value={newParty.openingBalance || ''}
                  onChange={(e) => setNewParty({ ...newParty, openingBalance: parseFloat(e.target.value) || 0 })}
                />
              </div>
              <DialogFooter>
                <Button variant="outline" onClick={() => setShowAddParty(false)}>Cancel</Button>
                <Button
                  onClick={() => createParty.mutate(newParty)}
                  disabled={!newParty.partyName.trim() || createParty.isPending}
                >
                  {createParty.isPending ? 'Adding...' : 'Add Party'}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-4">
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 rounded-lg bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
                <TrendingUp className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total Receivable</p>
                <p className="text-xl font-bold text-green-600">{formatCurrency(summary?.totalReceivable ?? 0)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-4">
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 rounded-lg bg-red-100 dark:bg-red-900/30 flex items-center justify-center">
                <TrendingDown className="h-5 w-5 text-red-600" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total Payable</p>
                <p className="text-xl font-bold text-red-600">{formatCurrency(summary?.totalPayable ?? 0)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-4">
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 rounded-lg bg-primary/10 flex items-center justify-center">
                <Wallet className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Net Balance</p>
                <p className="text-xl font-bold">{formatCurrency(summary?.netBalance ?? 0)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-4">
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
                <Users className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total Parties</p>
                <p className="text-xl font-bold">{summary?.totalParties ?? 0}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Search and Filter */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search parties..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-10"
          />
        </div>
        <Tabs value={activeTab} onValueChange={setActiveTab}>
          <TabsList>
            <TabsTrigger value="all">All</TabsTrigger>
            <TabsTrigger value="customer">Customers</TabsTrigger>
            <TabsTrigger value="supplier">Suppliers</TabsTrigger>
          </TabsList>
        </Tabs>
      </div>

      {/* Parties List */}
      <div className="space-y-3">
        {isLoading ? (
          Array.from({ length: 5 }).map((_, i) => (
            <Card key={i} className="animate-pulse">
              <CardContent className="py-4">
                <div className="flex items-center gap-4">
                  <div className="h-10 w-10 rounded-full bg-muted" />
                  <div className="flex-1 space-y-2">
                    <div className="h-4 w-32 bg-muted rounded" />
                    <div className="h-3 w-24 bg-muted rounded" />
                  </div>
                  <div className="h-6 w-20 bg-muted rounded" />
                </div>
              </CardContent>
            </Card>
          ))
        ) : parties.length === 0 ? (
          <Card className="py-16 text-center">
            <CardContent>
              <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-muted flex items-center justify-center">
                <Receipt className="h-8 w-8 text-muted-foreground" />
              </div>
              <h3 className="text-lg font-semibold mb-1">No khata records yet</h3>
              <p className="text-muted-foreground text-sm mb-4">
                Start by adding a customer or supplier to track their transactions.
              </p>
              <Button onClick={() => setShowAddParty(true)} className="gap-1.5">
                <Plus className="h-4 w-4" /> Add Your First Party
              </Button>
            </CardContent>
          </Card>
        ) : (
          <AnimatePresence>
            {parties.map((party: any, index: number) => (
              <motion.div
                key={party.id}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.03 }}
              >
                <Card className="hover:shadow-sm transition-shadow cursor-pointer">
                  <CardContent className="py-4">
                    <div className="flex items-center gap-4">
                      <Avatar className="h-10 w-10">
                        <AvatarFallback className={party.type === 'customer' ? 'bg-green-100 text-green-700' : 'bg-blue-100 text-blue-700'}>
                          {party.partyName?.[0]?.toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          <span className="font-medium truncate">{party.partyName}</span>
                          <Badge variant="outline" className="text-xs">
                            {party.type === 'customer' ? 'Customer' : 'Supplier'}
                          </Badge>
                        </div>
                        {party.partyPhone && (
                          <div className="flex items-center gap-1 text-xs text-muted-foreground mt-0.5">
                            <Phone className="h-3 w-3" />
                            {party.partyPhone}
                          </div>
                        )}
                      </div>
                      <div className="text-right">
                        <div className={`text-lg font-bold ${party.balance >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                          {formatCurrency(Math.abs(party.balance ?? 0))}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          {party.balance >= 0 ? 'will receive' : 'will pay'}
                        </div>
                      </div>
                      <div className="flex items-center gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-8 w-8 text-green-600"
                          onClick={(e) => {
                            e.stopPropagation();
                            setShowAddEntry(party.id);
                            setNewEntry({ amount: 0, type: 'credit', description: '' });
                          }}
                          title="Received payment"
                        >
                          <ArrowDownLeft className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-8 w-8 text-red-600"
                          onClick={(e) => {
                            e.stopPropagation();
                            setShowAddEntry(party.id);
                            setNewEntry({ amount: 0, type: 'debit', description: '' });
                          }}
                          title="Gave credit"
                        >
                          <ArrowUpRight className="h-4 w-4" />
                        </Button>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon" className="h-8 w-8">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem>View details</DropdownMenuItem>
                            <DropdownMenuItem onClick={() => sendReminder.mutate(party.id)}>
                              <Send className="h-4 w-4 mr-2" /> Send reminder
                            </DropdownMenuItem>
                            <DropdownMenuItem>
                              <Download className="h-4 w-4 mr-2" /> Export PDF
                            </DropdownMenuItem>
                            <DropdownMenuItem className="text-destructive">Delete</DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </motion.div>
            ))}
          </AnimatePresence>
        )}
      </div>

      {/* Add Entry Dialog */}
      <Dialog open={!!showAddEntry} onOpenChange={() => setShowAddEntry(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {newEntry.type === 'credit' ? 'Record Payment Received' : 'Record Credit Given'}
            </DialogTitle>
            <DialogDescription>
              {newEntry.type === 'credit'
                ? 'Record a payment you received from this party.'
                : 'Record credit/goods given to this party.'}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="flex gap-2">
              <Button
                variant={newEntry.type === 'credit' ? 'default' : 'outline'}
                className="flex-1 gap-1.5"
                onClick={() => setNewEntry({ ...newEntry, type: 'credit' })}
              >
                <ArrowDownLeft className="h-4 w-4" /> Received
              </Button>
              <Button
                variant={newEntry.type === 'debit' ? 'default' : 'outline'}
                className="flex-1 gap-1.5"
                onClick={() => setNewEntry({ ...newEntry, type: 'debit' })}
              >
                <ArrowUpRight className="h-4 w-4" /> Given
              </Button>
            </div>
            <Input
              type="number"
              placeholder="Amount"
              value={newEntry.amount || ''}
              onChange={(e) => setNewEntry({ ...newEntry, amount: parseFloat(e.target.value) || 0 })}
              className="text-lg"
            />
            <Input
              placeholder="Description (e.g., 2 bags cement)"
              value={newEntry.description}
              onChange={(e) => setNewEntry({ ...newEntry, description: e.target.value })}
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowAddEntry(null)}>Cancel</Button>
            <Button
              onClick={() => showAddEntry && addEntry.mutate({ partyId: showAddEntry, data: newEntry })}
              disabled={!newEntry.amount || !newEntry.description.trim() || addEntry.isPending}
              className={newEntry.type === 'credit' ? 'bg-green-600 hover:bg-green-700' : 'bg-red-600 hover:bg-red-700'}
            >
              {addEntry.isPending ? 'Saving...' : 'Save Entry'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
