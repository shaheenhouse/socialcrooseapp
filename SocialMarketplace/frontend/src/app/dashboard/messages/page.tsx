"use client";

import { useState, useRef, useEffect } from "react";
import { useTranslations } from "next-intl";
import { motion, AnimatePresence } from "framer-motion";
import {
  MessageCircle,
  Search,
  MoreVertical,
  Send,
  Paperclip,
  Smile,
  Phone,
  Video,
  Info,
  Check,
  CheckCheck,
  Image as ImageIcon,
  File,
  Archive,
  Bell,
  BellOff,
  Trash2,
  ChevronLeft,
} from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

// Mock data for conversations
const mockConversations = [
  {
    id: "1",
    name: "Sarah Johnson",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=sarah",
    lastMessage: "That sounds great! Let me know when you're free to discuss.",
    lastMessageAt: "2024-02-26T14:30:00",
    unreadCount: 2,
    isOnline: true,
    type: "direct",
  },
  {
    id: "2",
    name: "Project Alpha Team",
    avatar: "https://api.dicebear.com/7.x/shapes/svg?seed=alpha",
    lastMessage: "Ahmed: The new designs are ready for review",
    lastMessageAt: "2024-02-26T12:15:00",
    unreadCount: 5,
    isOnline: false,
    type: "group",
    participants: 4,
  },
  {
    id: "3",
    name: "Ahmed Khan",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=ahmed",
    lastMessage: "Thanks for the update!",
    lastMessageAt: "2024-02-26T10:00:00",
    unreadCount: 0,
    isOnline: true,
    type: "direct",
  },
  {
    id: "4",
    name: "Maria Garcia",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=maria",
    lastMessage: "I'll send you the files tomorrow",
    lastMessageAt: "2024-02-25T18:45:00",
    unreadCount: 0,
    isOnline: false,
    type: "direct",
  },
  {
    id: "5",
    name: "Support - Order #12345",
    avatar: "https://api.dicebear.com/7.x/shapes/svg?seed=support",
    lastMessage: "Your issue has been resolved",
    lastMessageAt: "2024-02-24T09:30:00",
    unreadCount: 0,
    isOnline: false,
    type: "support",
  },
];

const mockMessages = [
  {
    id: "m1",
    senderId: "other",
    content: "Hi! I saw your profile and I'm really impressed with your work.",
    timestamp: "2024-02-26T14:00:00",
    status: "read",
  },
  {
    id: "m2",
    senderId: "me",
    content: "Thank you! I appreciate that. What project did you have in mind?",
    timestamp: "2024-02-26T14:05:00",
    status: "read",
  },
  {
    id: "m3",
    senderId: "other",
    content: "We're looking for a full-stack developer to help with our new marketplace platform. Would you be interested in discussing further?",
    timestamp: "2024-02-26T14:10:00",
    status: "read",
  },
  {
    id: "m4",
    senderId: "me",
    content: "Absolutely! I'd love to hear more about the project. What's the tech stack you're using?",
    timestamp: "2024-02-26T14:15:00",
    status: "read",
  },
  {
    id: "m5",
    senderId: "other",
    content: "We're using Next.js for the frontend and .NET Core for the backend. PostgreSQL for the database. The project involves building a complete e-commerce solution with social features.",
    timestamp: "2024-02-26T14:20:00",
    status: "read",
  },
  {
    id: "m6",
    senderId: "other",
    content: "That sounds great! Let me know when you're free to discuss.",
    timestamp: "2024-02-26T14:30:00",
    status: "delivered",
  },
];

function formatMessageTime(timestamp: string) {
  const date = new Date(timestamp);
  const now = new Date();
  const diffDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));

  if (diffDays === 0) {
    return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
  } else if (diffDays === 1) {
    return "Yesterday";
  } else if (diffDays < 7) {
    return date.toLocaleDateString([], { weekday: "short" });
  }
  return date.toLocaleDateString([], { month: "short", day: "numeric" });
}

export default function MessagesPage() {
  const t = useTranslations();
  const [selectedConversation, setSelectedConversation] = useState<string | null>("1");
  const [searchQuery, setSearchQuery] = useState("");
  const [messageInput, setMessageInput] = useState("");
  const [showMobileChat, setShowMobileChat] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const activeConversation = mockConversations.find((c) => c.id === selectedConversation);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [selectedConversation]);

  const handleSendMessage = () => {
    if (!messageInput.trim()) return;
    // In production, this would send the message via API
    console.log("Sending message:", messageInput);
    setMessageInput("");
  };

  const handleSelectConversation = (id: string) => {
    setSelectedConversation(id);
    setShowMobileChat(true);
  };

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
          {/* Header */}
          <div className="p-4 border-b">
            <h1 className="text-xl font-bold mb-4">Messages</h1>
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

          {/* Conversations */}
          <ScrollArea className="flex-1">
            <div className="p-2">
              {mockConversations.map((conversation) => (
                <motion.button
                  key={conversation.id}
                  whileHover={{ scale: 1.01 }}
                  whileTap={{ scale: 0.99 }}
                  onClick={() => handleSelectConversation(conversation.id)}
                  className={cn(
                    "w-full p-3 rounded-lg flex items-center gap-3 transition-colors text-left",
                    selectedConversation === conversation.id
                      ? "bg-primary/10 border border-primary/20"
                      : "hover:bg-muted/50"
                  )}
                >
                  <div className="relative">
                    <Avatar className="h-12 w-12">
                      <AvatarImage src={conversation.avatar} />
                      <AvatarFallback>{conversation.name[0]}</AvatarFallback>
                    </Avatar>
                    {conversation.isOnline && (
                      <span className="absolute bottom-0 right-0 h-3 w-3 rounded-full bg-green-500 border-2 border-background" />
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between">
                      <h3 className="font-medium truncate">{conversation.name}</h3>
                      <span className="text-xs text-muted-foreground">
                        {formatMessageTime(conversation.lastMessageAt)}
                      </span>
                    </div>
                    <div className="flex items-center justify-between gap-2">
                      <p className="text-sm text-muted-foreground truncate">
                        {conversation.lastMessage}
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
                    variant="ghost"
                    size="icon"
                    className="md:hidden"
                    onClick={() => setShowMobileChat(false)}
                  >
                    <ChevronLeft className="h-5 w-5" />
                  </Button>
                  <div className="relative">
                    <Avatar className="h-10 w-10">
                      <AvatarImage src={activeConversation.avatar} />
                      <AvatarFallback>{activeConversation.name[0]}</AvatarFallback>
                    </Avatar>
                    {activeConversation.isOnline && (
                      <span className="absolute bottom-0 right-0 h-2.5 w-2.5 rounded-full bg-green-500 border-2 border-background" />
                    )}
                  </div>
                  <div>
                    <h2 className="font-semibold">{activeConversation.name}</h2>
                    <p className="text-xs text-muted-foreground">
                      {activeConversation.isOnline ? "Online" : "Last seen recently"}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <TooltipProvider>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Button variant="ghost" size="icon">
                          <Phone className="h-5 w-5" />
                        </Button>
                      </TooltipTrigger>
                      <TooltipContent>Voice call</TooltipContent>
                    </Tooltip>
                  </TooltipProvider>
                  <TooltipProvider>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Button variant="ghost" size="icon">
                          <Video className="h-5 w-5" />
                        </Button>
                      </TooltipTrigger>
                      <TooltipContent>Video call</TooltipContent>
                    </Tooltip>
                  </TooltipProvider>
                  <TooltipProvider>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Button variant="ghost" size="icon">
                          <Info className="h-5 w-5" />
                        </Button>
                      </TooltipTrigger>
                      <TooltipContent>Conversation info</TooltipContent>
                    </Tooltip>
                  </TooltipProvider>
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="icon">
                        <MoreVertical className="h-5 w-5" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem>
                        <BellOff className="h-4 w-4 mr-2" />
                        Mute notifications
                      </DropdownMenuItem>
                      <DropdownMenuItem>
                        <Archive className="h-4 w-4 mr-2" />
                        Archive conversation
                      </DropdownMenuItem>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem className="text-destructive">
                        <Trash2 className="h-4 w-4 mr-2" />
                        Delete conversation
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
              </div>

              {/* Messages */}
              <ScrollArea className="flex-1 p-4">
                <div className="space-y-4 max-w-3xl mx-auto">
                  {mockMessages.map((message, index) => {
                    const isMe = message.senderId === "me";
                    const showDate =
                      index === 0 ||
                      new Date(message.timestamp).toDateString() !==
                        new Date(mockMessages[index - 1].timestamp).toDateString();

                    return (
                      <div key={message.id}>
                        {showDate && (
                          <div className="flex items-center justify-center my-4">
                            <Separator className="flex-1" />
                            <span className="px-4 text-xs text-muted-foreground">
                              {new Date(message.timestamp).toLocaleDateString([], {
                                weekday: "long",
                                month: "short",
                                day: "numeric",
                              })}
                            </span>
                            <Separator className="flex-1" />
                          </div>
                        )}
                        <motion.div
                          initial={{ opacity: 0, y: 10 }}
                          animate={{ opacity: 1, y: 0 }}
                          className={cn(
                            "flex",
                            isMe ? "justify-end" : "justify-start"
                          )}
                        >
                          <div
                            className={cn(
                              "max-w-[70%] rounded-2xl px-4 py-2",
                              isMe
                                ? "bg-primary text-primary-foreground rounded-br-md"
                                : "bg-muted rounded-bl-md"
                            )}
                          >
                            <p className="text-sm">{message.content}</p>
                            <div
                              className={cn(
                                "flex items-center gap-1 mt-1",
                                isMe ? "justify-end" : "justify-start"
                              )}
                            >
                              <span
                                className={cn(
                                  "text-xs",
                                  isMe
                                    ? "text-primary-foreground/70"
                                    : "text-muted-foreground"
                                )}
                              >
                                {new Date(message.timestamp).toLocaleTimeString([], {
                                  hour: "2-digit",
                                  minute: "2-digit",
                                })}
                              </span>
                              {isMe && (
                                <span className="text-primary-foreground/70">
                                  {message.status === "read" ? (
                                    <CheckCheck className="h-3.5 w-3.5" />
                                  ) : (
                                    <Check className="h-3.5 w-3.5" />
                                  )}
                                </span>
                              )}
                            </div>
                          </div>
                        </motion.div>
                      </div>
                    );
                  })}
                  <div ref={messagesEndRef} />
                </div>
              </ScrollArea>

              {/* Input Area */}
              <div className="p-4 border-t bg-card/50 backdrop-blur-sm">
                <div className="max-w-3xl mx-auto flex items-center gap-2">
                  <TooltipProvider>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Button variant="ghost" size="icon">
                          <Paperclip className="h-5 w-5" />
                        </Button>
                      </TooltipTrigger>
                      <TooltipContent>Attach file</TooltipContent>
                    </Tooltip>
                  </TooltipProvider>
                  <TooltipProvider>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Button variant="ghost" size="icon">
                          <ImageIcon className="h-5 w-5" />
                        </Button>
                      </TooltipTrigger>
                      <TooltipContent>Send image</TooltipContent>
                    </Tooltip>
                  </TooltipProvider>
                  <Input
                    placeholder="Type a message..."
                    value={messageInput}
                    onChange={(e) => setMessageInput(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === "Enter" && !e.shiftKey) {
                        e.preventDefault();
                        handleSendMessage();
                      }
                    }}
                    className="flex-1"
                  />
                  <TooltipProvider>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Button variant="ghost" size="icon">
                          <Smile className="h-5 w-5" />
                        </Button>
                      </TooltipTrigger>
                      <TooltipContent>Emoji</TooltipContent>
                    </Tooltip>
                  </TooltipProvider>
                  <Button
                    onClick={handleSendMessage}
                    disabled={!messageInput.trim()}
                    className="gap-2"
                  >
                    <Send className="h-4 w-4" />
                    Send
                  </Button>
                </div>
              </div>
            </>
          ) : (
            <div className="flex-1 flex items-center justify-center">
              <div className="text-center">
                <MessageCircle className="h-16 w-16 mx-auto text-muted-foreground/50 mb-4" />
                <h2 className="text-xl font-semibold mb-2">Your Messages</h2>
                <p className="text-muted-foreground">
                  Select a conversation to start chatting
                </p>
              </div>
            </div>
          )}
        </motion.div>
      </div>
    </div>
  );
}
