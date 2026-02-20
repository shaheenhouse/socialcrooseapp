"use client";

import { useState, useRef, useEffect, useCallback } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { motion, AnimatePresence } from "framer-motion";
import {
  MessageCircle, Search, MoreVertical, Send, Paperclip,
  Smile, Phone, Video, Info, Check, CheckCheck,
  Image as ImageIcon, Archive, BellOff, Trash2, ChevronLeft,
  Plus, Users, Loader2,
} from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem,
  DropdownMenuSeparator, DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Tooltip, TooltipContent, TooltipProvider, TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
import { messageApi } from "@/lib/api";
import { useAuthStore } from "@/store/auth-store";
import { useChatHub } from "@/hooks/use-signalr";
import { formatRelativeTime } from "@/lib/utils";

interface Conversation {
  id: string;
  title: string;
  type: string;
  avatarUrl?: string;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount: number;
  isOnline?: boolean;
  participantCount?: number;
}

interface Message {
  id: string;
  senderId: string;
  senderName?: string;
  content: string;
  type: string;
  attachmentUrl?: string;
  createdAt: string;
  isEdited: boolean;
  status?: string;
}

export default function MessagesPage() {
  const { user } = useAuthStore();
  const queryClient = useQueryClient();
  const { connected, invoke, on } = useChatHub();
  const [selectedConversation, setSelectedConversation] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [messageInput, setMessageInput] = useState("");
  const [showMobileChat, setShowMobileChat] = useState(false);
  const [isTyping, setIsTyping] = useState<string | null>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const typingTimeoutRef = useRef<ReturnType<typeof setTimeout>>();

  const { data: conversationsData, isLoading: loadingConversations } = useQuery({
    queryKey: ["conversations"],
    queryFn: () => messageApi.getConversations({ pageSize: 50 }),
  });

  const conversations: Conversation[] = conversationsData?.data?.items ?? conversationsData?.data ?? [];

  const { data: messagesData, isLoading: loadingMessages } = useQuery({
    queryKey: ["messages", selectedConversation],
    queryFn: () => messageApi.getMessages(selectedConversation!, { pageSize: 100 }),
    enabled: !!selectedConversation,
  });

  const messages: Message[] = messagesData?.data?.items ?? messagesData?.data ?? [];
  const activeConversation = conversations.find((c) => c.id === selectedConversation);

  const sendMessageMutation = useMutation({
    mutationFn: (content: string) =>
      messageApi.sendMessage(selectedConversation!, { content, type: "text" }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["messages", selectedConversation] });
      queryClient.invalidateQueries({ queryKey: ["conversations"] });
    },
  });

  // SignalR real-time events
  useEffect(() => {
    if (!connected) return;

    const cleanups = [
      on("NewMessage", (msg: any) => {
        queryClient.invalidateQueries({ queryKey: ["messages"] });
        queryClient.invalidateQueries({ queryKey: ["conversations"] });
      }),
      on("NewDirectMessage", (msg: any) => {
        queryClient.invalidateQueries({ queryKey: ["messages"] });
        queryClient.invalidateQueries({ queryKey: ["conversations"] });
      }),
      on("MessageSent", () => {
        queryClient.invalidateQueries({ queryKey: ["messages", selectedConversation] });
      }),
      on("UserTyping", (userId: string, roomId: string) => {
        if (roomId === selectedConversation) {
          setIsTyping(userId);
          setTimeout(() => setIsTyping(null), 3000);
        }
      }),
      on("UserStoppedTyping", () => setIsTyping(null)),
      on("MessagesRead", () => {
        queryClient.invalidateQueries({ queryKey: ["messages", selectedConversation] });
      }),
    ];

    return () => cleanups.forEach((cleanup) => cleanup?.());
  }, [connected, on, queryClient, selectedConversation]);

  // Join room when conversation selected
  useEffect(() => {
    if (connected && selectedConversation) {
      invoke("JoinRoom", selectedConversation);
      invoke("MarkAsRead", selectedConversation, selectedConversation);
    }
  }, [connected, selectedConversation, invoke]);

  // Auto-scroll
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const handleSendMessage = useCallback(() => {
    if (!messageInput.trim() || !selectedConversation) return;
    const content = messageInput.trim();
    setMessageInput("");

    if (connected) {
      invoke("SendMessage", selectedConversation, content);
    }
    sendMessageMutation.mutate(content);
  }, [messageInput, selectedConversation, connected, invoke, sendMessageMutation]);

  const handleTyping = useCallback(() => {
    if (!connected || !selectedConversation) return;
    invoke("Typing", selectedConversation);
    if (typingTimeoutRef.current) clearTimeout(typingTimeoutRef.current);
    typingTimeoutRef.current = setTimeout(() => {
      invoke("StopTyping", selectedConversation);
    }, 2000);
  }, [connected, selectedConversation, invoke]);

  const handleSelectConversation = (id: string) => {
    setSelectedConversation(id);
    setShowMobileChat(true);
  };

  const markAsRead = useMutation({
    mutationFn: (id: string) => messageApi.markAsRead(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["conversations"] }),
  });

  const filteredConversations = conversations.filter((c) =>
    c.title?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="h-[calc(100vh-4rem)] bg-gradient-to-br from-background via-background to-primary/5">
      <div className="h-full flex">
        {/* Conversations List */}
        <motion.div
          initial={{ opacity: 0, x: -20 }}
          animate={{ opacity: 1, x: 0 }}
          className={cn(
            "w-full md:w-96 border-r bg-card/50 backdrop-blur-sm flex flex-col",
            showMobileChat && "hidden md:flex"
          )}
        >
          <div className="p-4 border-b">
            <div className="flex items-center justify-between mb-4">
              <h1 className="text-xl font-bold">Messages</h1>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <Plus className="h-4 w-4" />
              </Button>
            </div>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search messages..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9"
              />
            </div>
          </div>

          <ScrollArea className="flex-1">
            <div className="p-2">
              {loadingConversations && (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                </div>
              )}
              {filteredConversations.map((conversation) => (
                <motion.button
                  key={conversation.id}
                  whileHover={{ scale: 1.01 }}
                  whileTap={{ scale: 0.99 }}
                  onClick={() => {
                    handleSelectConversation(conversation.id);
                    if (conversation.unreadCount > 0) markAsRead.mutate(conversation.id);
                  }}
                  className={cn(
                    "w-full p-3 rounded-lg flex items-center gap-3 transition-colors text-left",
                    selectedConversation === conversation.id
                      ? "bg-primary/10 border border-primary/20"
                      : "hover:bg-muted/50"
                  )}
                >
                  <div className="relative">
                    <Avatar className="h-12 w-12">
                      <AvatarImage src={conversation.avatarUrl} />
                      <AvatarFallback>{conversation.title?.[0] ?? "?"}</AvatarFallback>
                    </Avatar>
                    {conversation.isOnline && (
                      <span className="absolute bottom-0 right-0 h-3 w-3 rounded-full bg-green-500 border-2 border-background" />
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between">
                      <h3 className="font-medium truncate">{conversation.title}</h3>
                      {conversation.lastMessageAt && (
                        <span className="text-xs text-muted-foreground">
                          {formatRelativeTime(conversation.lastMessageAt)}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center justify-between gap-2">
                      <p className="text-sm text-muted-foreground truncate">
                        {conversation.lastMessage ?? "No messages yet"}
                      </p>
                      {conversation.unreadCount > 0 && (
                        <Badge className="h-5 min-w-[20px] px-1.5 justify-center">
                          {conversation.unreadCount}
                        </Badge>
                      )}
                    </div>
                  </div>
                </motion.button>
              ))}
              {!loadingConversations && filteredConversations.length === 0 && (
                <div className="text-center py-8 text-muted-foreground text-sm">
                  No conversations yet
                </div>
              )}
            </div>
          </ScrollArea>
        </motion.div>

        {/* Chat Area */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className={cn(
            "flex-1 flex flex-col",
            !showMobileChat && "hidden md:flex"
          )}
        >
          {activeConversation ? (
            <>
              {/* Chat Header */}
              <div className="p-4 border-b bg-card/50 backdrop-blur-sm flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Button
                    variant="ghost" size="icon" className="md:hidden"
                    onClick={() => setShowMobileChat(false)}
                  >
                    <ChevronLeft className="h-5 w-5" />
                  </Button>
                  <div className="relative">
                    <Avatar className="h-10 w-10">
                      <AvatarImage src={activeConversation.avatarUrl} />
                      <AvatarFallback>{activeConversation.title?.[0]}</AvatarFallback>
                    </Avatar>
                    {activeConversation.isOnline && (
                      <span className="absolute bottom-0 right-0 h-2.5 w-2.5 rounded-full bg-green-500 border-2 border-background" />
                    )}
                  </div>
                  <div>
                    <h2 className="font-semibold">{activeConversation.title}</h2>
                    <p className="text-xs text-muted-foreground">
                      {isTyping ? "typing..." : activeConversation.isOnline ? "Online" : "Offline"}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-1">
                  <TooltipProvider>
                    <Tooltip><TooltipTrigger asChild>
                      <Button variant="ghost" size="icon"><Phone className="h-5 w-5" /></Button>
                    </TooltipTrigger><TooltipContent>Voice call</TooltipContent></Tooltip>
                    <Tooltip><TooltipTrigger asChild>
                      <Button variant="ghost" size="icon"><Video className="h-5 w-5" /></Button>
                    </TooltipTrigger><TooltipContent>Video call</TooltipContent></Tooltip>
                    <Tooltip><TooltipTrigger asChild>
                      <Button variant="ghost" size="icon"><Info className="h-5 w-5" /></Button>
                    </TooltipTrigger><TooltipContent>Info</TooltipContent></Tooltip>
                  </TooltipProvider>
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="icon"><MoreVertical className="h-5 w-5" /></Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem><BellOff className="h-4 w-4 mr-2" />Mute</DropdownMenuItem>
                      <DropdownMenuItem><Archive className="h-4 w-4 mr-2" />Archive</DropdownMenuItem>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem className="text-destructive">
                        <Trash2 className="h-4 w-4 mr-2" />Delete
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
              </div>

              {/* Messages */}
              <ScrollArea className="flex-1 p-4">
                <div className="space-y-4 max-w-3xl mx-auto">
                  {loadingMessages && (
                    <div className="flex justify-center py-8">
                      <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                    </div>
                  )}
                  {messages.map((message, index) => {
                    const isMe = message.senderId === user?.id;
                    const showDate =
                      index === 0 ||
                      new Date(message.createdAt).toDateString() !==
                        new Date(messages[index - 1].createdAt).toDateString();

                    return (
                      <div key={message.id}>
                        {showDate && (
                          <div className="flex items-center justify-center my-4">
                            <Separator className="flex-1" />
                            <span className="px-4 text-xs text-muted-foreground">
                              {new Date(message.createdAt).toLocaleDateString([], {
                                weekday: "long", month: "short", day: "numeric",
                              })}
                            </span>
                            <Separator className="flex-1" />
                          </div>
                        )}
                        <motion.div
                          initial={{ opacity: 0, y: 10 }}
                          animate={{ opacity: 1, y: 0 }}
                          className={cn("flex", isMe ? "justify-end" : "justify-start")}
                        >
                          <div className={cn(
                            "max-w-[70%] rounded-2xl px-4 py-2",
                            isMe
                              ? "bg-primary text-primary-foreground rounded-br-md"
                              : "bg-muted rounded-bl-md"
                          )}>
                            {!isMe && message.senderName && (
                              <p className="text-xs font-medium mb-0.5 opacity-70">{message.senderName}</p>
                            )}
                            <p className="text-sm">{message.content}</p>
                            <div className={cn(
                              "flex items-center gap-1 mt-1",
                              isMe ? "justify-end" : "justify-start"
                            )}>
                              <span className={cn(
                                "text-xs",
                                isMe ? "text-primary-foreground/70" : "text-muted-foreground"
                              )}>
                                {new Date(message.createdAt).toLocaleTimeString([], {
                                  hour: "2-digit", minute: "2-digit",
                                })}
                              </span>
                              {isMe && (
                                <span className="text-primary-foreground/70">
                                  {message.status === "read"
                                    ? <CheckCheck className="h-3.5 w-3.5" />
                                    : <Check className="h-3.5 w-3.5" />}
                                </span>
                              )}
                            </div>
                          </div>
                        </motion.div>
                      </div>
                    );
                  })}
                  {isTyping && (
                    <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="flex justify-start">
                      <div className="bg-muted rounded-2xl rounded-bl-md px-4 py-2">
                        <div className="flex gap-1">
                          <span className="w-2 h-2 bg-muted-foreground/50 rounded-full animate-bounce" />
                          <span className="w-2 h-2 bg-muted-foreground/50 rounded-full animate-bounce [animation-delay:0.1s]" />
                          <span className="w-2 h-2 bg-muted-foreground/50 rounded-full animate-bounce [animation-delay:0.2s]" />
                        </div>
                      </div>
                    </motion.div>
                  )}
                  <div ref={messagesEndRef} />
                </div>
              </ScrollArea>

              {/* Input Area */}
              <div className="p-4 border-t bg-card/50 backdrop-blur-sm">
                <div className="max-w-3xl mx-auto flex items-center gap-2">
                  <TooltipProvider>
                    <Tooltip><TooltipTrigger asChild>
                      <Button variant="ghost" size="icon"><Paperclip className="h-5 w-5" /></Button>
                    </TooltipTrigger><TooltipContent>Attach</TooltipContent></Tooltip>
                    <Tooltip><TooltipTrigger asChild>
                      <Button variant="ghost" size="icon"><ImageIcon className="h-5 w-5" /></Button>
                    </TooltipTrigger><TooltipContent>Image</TooltipContent></Tooltip>
                  </TooltipProvider>
                  <Input
                    placeholder="Type a message..."
                    value={messageInput}
                    onChange={(e) => {
                      setMessageInput(e.target.value);
                      handleTyping();
                    }}
                    onKeyDown={(e) => {
                      if (e.key === "Enter" && !e.shiftKey) {
                        e.preventDefault();
                        handleSendMessage();
                      }
                    }}
                    className="flex-1"
                  />
                  <TooltipProvider>
                    <Tooltip><TooltipTrigger asChild>
                      <Button variant="ghost" size="icon"><Smile className="h-5 w-5" /></Button>
                    </TooltipTrigger><TooltipContent>Emoji</TooltipContent></Tooltip>
                  </TooltipProvider>
                  <Button onClick={handleSendMessage} disabled={!messageInput.trim()} className="gap-2">
                    <Send className="h-4 w-4" /> Send
                  </Button>
                </div>
              </div>
            </>
          ) : (
            <div className="flex-1 flex items-center justify-center">
              <div className="text-center">
                <MessageCircle className="h-16 w-16 mx-auto text-muted-foreground/50 mb-4" />
                <h2 className="text-xl font-semibold mb-2">Your Messages</h2>
                <p className="text-muted-foreground">Select a conversation to start chatting</p>
                {connected && (
                  <Badge variant="outline" className="mt-3 text-green-600 border-green-300">
                    <span className="w-2 h-2 bg-green-500 rounded-full mr-2" /> Live
                  </Badge>
                )}
              </div>
            </div>
          )}
        </motion.div>
      </div>
    </div>
  );
}
