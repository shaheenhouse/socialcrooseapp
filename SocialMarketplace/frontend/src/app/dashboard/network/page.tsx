"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { motion, AnimatePresence } from "framer-motion";
import {
  Users,
  UserPlus,
  UserCheck,
  Clock,
  Send,
  Search,
  MoreHorizontal,
  Check,
  X,
  UserMinus,
  MessageCircle,
  Sparkles,
} from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

// Mock data for connections
const mockConnections = [
  {
    id: "1",
    name: "Sarah Johnson",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=sarah",
    headline: "Senior Full Stack Developer at TechCorp",
    mutualConnections: 12,
    connectedAt: "2024-01-15",
  },
  {
    id: "2",
    name: "Ahmed Khan",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=ahmed",
    headline: "Product Manager | Building the Future",
    mutualConnections: 8,
    connectedAt: "2024-02-01",
  },
  {
    id: "3",
    name: "Maria Garcia",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=maria",
    headline: "UX Designer & Creative Director",
    mutualConnections: 5,
    connectedAt: "2024-02-10",
  },
];

const mockPendingRequests = [
  {
    id: "4",
    name: "John Smith",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=john",
    headline: "CEO at StartupXYZ",
    mutualConnections: 3,
    message: "Hi! I'd love to connect and discuss potential collaboration opportunities.",
    requestedAt: "2024-02-25",
  },
  {
    id: "5",
    name: "Emily Chen",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=emily",
    headline: "Data Scientist | AI Enthusiast",
    mutualConnections: 7,
    requestedAt: "2024-02-26",
  },
];

const mockSuggestions = [
  {
    id: "6",
    name: "David Wilson",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=david",
    headline: "Senior Software Engineer at Google",
    mutualConnections: 15,
    reason: "Based on your profile",
  },
  {
    id: "7",
    name: "Lisa Anderson",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=lisa",
    headline: "Marketing Director | Growth Expert",
    mutualConnections: 9,
    reason: "Works at a company you follow",
  },
  {
    id: "8",
    name: "Michael Brown",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=michael",
    headline: "Freelance Designer & Illustrator",
    mutualConnections: 4,
    reason: "Viewed your profile",
  },
  {
    id: "9",
    name: "Jessica Taylor",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=jessica",
    headline: "Backend Developer | Node.js Expert",
    mutualConnections: 11,
    reason: "Similar skills",
  },
];

export default function NetworkPage() {
  const t = useTranslations();
  const [activeTab, setActiveTab] = useState("connections");
  const [searchQuery, setSearchQuery] = useState("");

  const stats = {
    connections: 248,
    pendingReceived: 5,
    pendingSent: 2,
    followers: 1024,
    following: 156,
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-primary/5 p-6">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex flex-col md:flex-row md:items-center md:justify-between gap-4"
        >
          <div>
            <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-purple-600 bg-clip-text text-transparent">
              My Network
            </h1>
            <p className="text-muted-foreground mt-1">
              Manage your professional connections and grow your network
            </p>
          </div>
          <div className="flex items-center gap-3">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search connections..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9 w-64"
              />
            </div>
          </div>
        </motion.div>

        {/* Stats Cards */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="grid grid-cols-2 md:grid-cols-5 gap-4"
        >
          <Card className="bg-gradient-to-br from-blue-500/10 to-blue-600/5 border-blue-500/20">
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-blue-500/20">
                  <Users className="h-5 w-5 text-blue-500" />
                </div>
                <div>
                  <p className="text-2xl font-bold">{stats.connections}</p>
                  <p className="text-xs text-muted-foreground">Connections</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card className="bg-gradient-to-br from-orange-500/10 to-orange-600/5 border-orange-500/20">
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-orange-500/20">
                  <Clock className="h-5 w-5 text-orange-500" />
                </div>
                <div>
                  <p className="text-2xl font-bold">{stats.pendingReceived}</p>
                  <p className="text-xs text-muted-foreground">Pending</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card className="bg-gradient-to-br from-purple-500/10 to-purple-600/5 border-purple-500/20">
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-purple-500/20">
                  <Send className="h-5 w-5 text-purple-500" />
                </div>
                <div>
                  <p className="text-2xl font-bold">{stats.pendingSent}</p>
                  <p className="text-xs text-muted-foreground">Sent</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card className="bg-gradient-to-br from-green-500/10 to-green-600/5 border-green-500/20">
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-green-500/20">
                  <UserCheck className="h-5 w-5 text-green-500" />
                </div>
                <div>
                  <p className="text-2xl font-bold">{stats.followers}</p>
                  <p className="text-xs text-muted-foreground">Followers</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card className="bg-gradient-to-br from-pink-500/10 to-pink-600/5 border-pink-500/20">
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-pink-500/20">
                  <UserPlus className="h-5 w-5 text-pink-500" />
                </div>
                <div>
                  <p className="text-2xl font-bold">{stats.following}</p>
                  <p className="text-xs text-muted-foreground">Following</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </motion.div>

        {/* Main Content */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Connections List */}
          <div className="lg:col-span-2">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2 }}
            >
              <Tabs value={activeTab} onValueChange={setActiveTab}>
                <TabsList className="mb-4">
                  <TabsTrigger value="connections" className="gap-2">
                    <Users className="h-4 w-4" />
                    Connections
                  </TabsTrigger>
                  <TabsTrigger value="pending" className="gap-2">
                    <Clock className="h-4 w-4" />
                    Pending
                    {stats.pendingReceived > 0 && (
                      <Badge variant="destructive" className="ml-1 h-5 w-5 p-0 justify-center">
                        {stats.pendingReceived}
                      </Badge>
                    )}
                  </TabsTrigger>
                  <TabsTrigger value="sent" className="gap-2">
                    <Send className="h-4 w-4" />
                    Sent
                  </TabsTrigger>
                </TabsList>

                <TabsContent value="connections" className="space-y-3">
                  <AnimatePresence mode="popLayout">
                    {mockConnections.map((connection, index) => (
                      <motion.div
                        key={connection.id}
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0, x: -100 }}
                        transition={{ delay: index * 0.05 }}
                      >
                        <Card className="hover:shadow-lg transition-shadow">
                          <CardContent className="p-4">
                            <div className="flex items-center gap-4">
                              <Avatar className="h-14 w-14 ring-2 ring-primary/20">
                                <AvatarImage src={connection.avatar} />
                                <AvatarFallback>{connection.name[0]}</AvatarFallback>
                              </Avatar>
                              <div className="flex-1 min-w-0">
                                <h3 className="font-semibold truncate">{connection.name}</h3>
                                <p className="text-sm text-muted-foreground truncate">
                                  {connection.headline}
                                </p>
                                <p className="text-xs text-muted-foreground mt-1">
                                  {connection.mutualConnections} mutual connections
                                </p>
                              </div>
                              <div className="flex items-center gap-2">
                                <Button variant="outline" size="sm" className="gap-2">
                                  <MessageCircle className="h-4 w-4" />
                                  Message
                                </Button>
                                <DropdownMenu>
                                  <DropdownMenuTrigger asChild>
                                    <Button variant="ghost" size="icon">
                                      <MoreHorizontal className="h-4 w-4" />
                                    </Button>
                                  </DropdownMenuTrigger>
                                  <DropdownMenuContent align="end">
                                    <DropdownMenuItem>View Profile</DropdownMenuItem>
                                    <DropdownMenuItem>Share Profile</DropdownMenuItem>
                                    <DropdownMenuSeparator />
                                    <DropdownMenuItem className="text-destructive">
                                      <UserMinus className="h-4 w-4 mr-2" />
                                      Remove Connection
                                    </DropdownMenuItem>
                                  </DropdownMenuContent>
                                </DropdownMenu>
                              </div>
                            </div>
                          </CardContent>
                        </Card>
                      </motion.div>
                    ))}
                  </AnimatePresence>
                </TabsContent>

                <TabsContent value="pending" className="space-y-3">
                  <AnimatePresence mode="popLayout">
                    {mockPendingRequests.map((request, index) => (
                      <motion.div
                        key={request.id}
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0, x: -100 }}
                        transition={{ delay: index * 0.05 }}
                      >
                        <Card className="hover:shadow-lg transition-shadow border-orange-500/20">
                          <CardContent className="p-4">
                            <div className="flex items-start gap-4">
                              <Avatar className="h-14 w-14 ring-2 ring-orange-500/20">
                                <AvatarImage src={request.avatar} />
                                <AvatarFallback>{request.name[0]}</AvatarFallback>
                              </Avatar>
                              <div className="flex-1 min-w-0">
                                <h3 className="font-semibold truncate">{request.name}</h3>
                                <p className="text-sm text-muted-foreground truncate">
                                  {request.headline}
                                </p>
                                <p className="text-xs text-muted-foreground mt-1">
                                  {request.mutualConnections} mutual connections
                                </p>
                                {request.message && (
                                  <p className="text-sm mt-2 p-2 rounded-lg bg-muted/50 italic">
                                    &ldquo;{request.message}&rdquo;
                                  </p>
                                )}
                              </div>
                              <div className="flex flex-col gap-2">
                                <Button size="sm" className="gap-2">
                                  <Check className="h-4 w-4" />
                                  Accept
                                </Button>
                                <Button variant="outline" size="sm" className="gap-2">
                                  <X className="h-4 w-4" />
                                  Ignore
                                </Button>
                              </div>
                            </div>
                          </CardContent>
                        </Card>
                      </motion.div>
                    ))}
                  </AnimatePresence>
                </TabsContent>

                <TabsContent value="sent" className="space-y-3">
                  <div className="text-center py-12 text-muted-foreground">
                    <Send className="h-12 w-12 mx-auto mb-4 opacity-50" />
                    <p>No pending sent requests</p>
                  </div>
                </TabsContent>
              </Tabs>
            </motion.div>
          </div>

          {/* Suggestions Sidebar */}
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.3 }}
          >
            <Card className="sticky top-6">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Sparkles className="h-5 w-5 text-yellow-500" />
                  People You May Know
                </CardTitle>
                <CardDescription>
                  Expand your network with these suggestions
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {mockSuggestions.map((suggestion, index) => (
                  <motion.div
                    key={suggestion.id}
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: 0.3 + index * 0.1 }}
                    className="flex items-center gap-3 p-3 rounded-lg hover:bg-muted/50 transition-colors"
                  >
                    <Avatar className="h-12 w-12">
                      <AvatarImage src={suggestion.avatar} />
                      <AvatarFallback>{suggestion.name[0]}</AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                      <h4 className="font-medium text-sm truncate">{suggestion.name}</h4>
                      <p className="text-xs text-muted-foreground truncate">
                        {suggestion.headline}
                      </p>
                      <p className="text-xs text-primary mt-1">{suggestion.reason}</p>
                    </div>
                    <Button size="sm" variant="outline" className="shrink-0">
                      <UserPlus className="h-4 w-4" />
                    </Button>
                  </motion.div>
                ))}
                <Button variant="ghost" className="w-full mt-2">
                  See all suggestions
                </Button>
              </CardContent>
            </Card>
          </motion.div>
        </div>
      </div>
    </div>
  );
}
