'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Heart, MessageCircle, Share2, Bookmark, MoreHorizontal,
  ImagePlus, Video, FileText, BarChart3, Send, Globe, Lock, Users,
  ThumbsUp, Smile, Award, Lightbulb, Flame, X,
} from 'lucide-react';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Input } from '@/components/ui/input';
import { postApi } from '@/lib/api';
import { useAuthStore } from '@/store/auth-store';
import { formatRelativeTime } from '@/lib/utils';

const reactionTypes = [
  { type: 'like', icon: ThumbsUp, label: 'Like', color: 'text-blue-500' },
  { type: 'love', icon: Heart, label: 'Love', color: 'text-red-500' },
  { type: 'celebrate', icon: Award, label: 'Celebrate', color: 'text-green-500' },
  { type: 'insightful', icon: Lightbulb, label: 'Insightful', color: 'text-yellow-500' },
  { type: 'funny', icon: Smile, label: 'Funny', color: 'text-orange-500' },
  { type: 'hot', icon: Flame, label: 'Hot', color: 'text-pink-500' },
];

export default function FeedPage() {
  const { user } = useAuthStore();
  const queryClient = useQueryClient();
  const [postContent, setPostContent] = useState('');
  const [showReactions, setShowReactions] = useState<string | null>(null);
  const [commentingOn, setCommentingOn] = useState<string | null>(null);
  const [commentText, setCommentText] = useState('');

  const { data: feedData, isLoading } = useQuery({
    queryKey: ['feed'],
    queryFn: () => postApi.getFeed({ pageSize: 20 }),
  });

  const createPost = useMutation({
    mutationFn: (content: string) => {
      const formData = new FormData();
      formData.append('content', content);
      formData.append('type', 'text');
      return postApi.create(formData);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['feed'] });
      setPostContent('');
    },
  });

  const reactToPost = useMutation({
    mutationFn: ({ postId, type }: { postId: string; type: string }) =>
      postApi.react(postId, type),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['feed'] }),
  });

  const addComment = useMutation({
    mutationFn: ({ postId, content }: { postId: string; content: string }) =>
      postApi.addComment(postId, { content }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['feed'] });
      setCommentText('');
      setCommentingOn(null);
    },
  });

  const posts = feedData?.data?.items ?? feedData?.data ?? [];

  return (
    <div className="max-w-2xl mx-auto space-y-4">
      {/* Create Post Card */}
      <Card>
        <CardContent className="pt-4">
          <div className="flex gap-3">
            <Avatar className="h-10 w-10">
              <AvatarImage src={user?.avatarUrl} />
              <AvatarFallback>{user?.firstName?.[0]}{user?.lastName?.[0]}</AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <Input
                placeholder="What's on your mind?"
                value={postContent}
                onChange={(e) => setPostContent(e.target.value)}
                className="bg-muted/50 border-none focus-visible:ring-1"
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && postContent.trim()) {
                    createPost.mutate(postContent);
                  }
                }}
              />
              <div className="flex items-center justify-between mt-3">
                <div className="flex gap-1">
                  <Button variant="ghost" size="sm" className="text-muted-foreground gap-1.5">
                    <ImagePlus className="h-4 w-4 text-green-500" /> Photo
                  </Button>
                  <Button variant="ghost" size="sm" className="text-muted-foreground gap-1.5">
                    <Video className="h-4 w-4 text-blue-500" /> Video
                  </Button>
                  <Button variant="ghost" size="sm" className="text-muted-foreground gap-1.5 hidden sm:inline-flex">
                    <FileText className="h-4 w-4 text-orange-500" /> Article
                  </Button>
                  <Button variant="ghost" size="sm" className="text-muted-foreground gap-1.5 hidden sm:inline-flex">
                    <BarChart3 className="h-4 w-4 text-purple-500" /> Poll
                  </Button>
                </div>
                <Button
                  size="sm"
                  disabled={!postContent.trim() || createPost.isPending}
                  onClick={() => createPost.mutate(postContent)}
                  className="gap-1.5"
                >
                  <Send className="h-3.5 w-3.5" /> Post
                </Button>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Loading Skeleton */}
      {isLoading && (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <Card key={i} className="animate-pulse">
              <CardHeader className="flex-row gap-3 space-y-0">
                <div className="h-10 w-10 rounded-full bg-muted" />
                <div className="space-y-2 flex-1">
                  <div className="h-4 w-32 bg-muted rounded" />
                  <div className="h-3 w-24 bg-muted rounded" />
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  <div className="h-4 w-full bg-muted rounded" />
                  <div className="h-4 w-3/4 bg-muted rounded" />
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Feed Posts */}
      <AnimatePresence>
        {posts.map((post: any, index: number) => (
          <motion.div
            key={post.id}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.05 }}
          >
            <Card className="hover:shadow-sm transition-shadow">
              <CardHeader className="flex-row gap-3 space-y-0 pb-2">
                <Avatar className="h-10 w-10">
                  <AvatarImage src={post.author?.avatarUrl} />
                  <AvatarFallback>
                    {post.author?.firstName?.[0]}{post.author?.lastName?.[0]}
                  </AvatarFallback>
                </Avatar>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <span className="font-semibold text-sm truncate">
                      {post.author?.firstName} {post.author?.lastName}
                    </span>
                    {post.author?.isVerified && (
                      <Badge variant="secondary" className="text-xs px-1.5 py-0">Pro</Badge>
                    )}
                  </div>
                  <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                    <span>{post.author?.headline || 'Member'}</span>
                    <span>·</span>
                    <span>{formatRelativeTime(post.createdAt)}</span>
                    <Globe className="h-3 w-3" />
                  </div>
                </div>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="icon" className="h-8 w-8">
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem>Save post</DropdownMenuItem>
                    <DropdownMenuItem>Copy link</DropdownMenuItem>
                    <DropdownMenuItem>Hide post</DropdownMenuItem>
                    <DropdownMenuItem className="text-destructive">Report</DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </CardHeader>

              <CardContent className="space-y-3">
                <p className="text-sm leading-relaxed whitespace-pre-wrap">{post.content}</p>

                {post.imageUrl && (
                  <div className="rounded-lg overflow-hidden bg-muted -mx-4">
                    <img src={post.imageUrl} alt="" className="w-full object-cover max-h-[500px]" />
                  </div>
                )}

                {/* Reaction counts */}
                {(post.reactionsCount > 0 || post.commentsCount > 0) && (
                  <div className="flex items-center justify-between text-xs text-muted-foreground">
                    <span>{post.reactionsCount || 0} reactions</span>
                    <span>{post.commentsCount || 0} comments · {post.sharesCount || 0} shares</span>
                  </div>
                )}

                <Separator />

                {/* Action buttons */}
                <div className="flex items-center justify-between -mx-2">
                  <div className="relative">
                    <Button
                      variant="ghost"
                      size="sm"
                      className="gap-1.5 text-muted-foreground"
                      onMouseEnter={() => setShowReactions(post.id)}
                      onMouseLeave={() => setShowReactions(null)}
                      onClick={() => reactToPost.mutate({ postId: post.id, type: 'like' })}
                    >
                      <ThumbsUp className="h-4 w-4" /> Like
                    </Button>
                    <AnimatePresence>
                      {showReactions === post.id && (
                        <motion.div
                          initial={{ opacity: 0, y: 10, scale: 0.9 }}
                          animate={{ opacity: 1, y: 0, scale: 1 }}
                          exit={{ opacity: 0, y: 10, scale: 0.9 }}
                          className="absolute bottom-full left-0 mb-1 flex gap-1 bg-card rounded-full shadow-lg border p-1.5 z-10"
                          onMouseEnter={() => setShowReactions(post.id)}
                          onMouseLeave={() => setShowReactions(null)}
                        >
                          {reactionTypes.map((r) => (
                            <button
                              key={r.type}
                              onClick={(e) => {
                                e.stopPropagation();
                                reactToPost.mutate({ postId: post.id, type: r.type });
                                setShowReactions(null);
                              }}
                              className="p-1.5 hover:scale-125 transition-transform rounded-full hover:bg-muted"
                              title={r.label}
                            >
                              <r.icon className={`h-5 w-5 ${r.color}`} />
                            </button>
                          ))}
                        </motion.div>
                      )}
                    </AnimatePresence>
                  </div>
                  <Button
                    variant="ghost" size="sm" className="gap-1.5 text-muted-foreground"
                    onClick={() => setCommentingOn(commentingOn === post.id ? null : post.id)}
                  >
                    <MessageCircle className="h-4 w-4" /> Comment
                  </Button>
                  <Button variant="ghost" size="sm" className="gap-1.5 text-muted-foreground">
                    <Share2 className="h-4 w-4" /> Share
                  </Button>
                  <Button variant="ghost" size="sm" className="gap-1.5 text-muted-foreground">
                    <Bookmark className="h-4 w-4" /> Save
                  </Button>
                </div>

                {/* Inline Comment Box */}
                <AnimatePresence>
                  {commentingOn === post.id && (
                    <motion.div
                      initial={{ opacity: 0, height: 0 }}
                      animate={{ opacity: 1, height: 'auto' }}
                      exit={{ opacity: 0, height: 0 }}
                      className="overflow-hidden"
                    >
                      <Separator className="mb-3" />
                      <div className="flex gap-2">
                        <Avatar className="h-8 w-8">
                          <AvatarImage src={user?.avatarUrl} />
                          <AvatarFallback className="text-xs">
                            {user?.firstName?.[0]}{user?.lastName?.[0]}
                          </AvatarFallback>
                        </Avatar>
                        <Input
                          placeholder="Write a comment..."
                          value={commentingOn === post.id ? commentText : ''}
                          onChange={(e) => setCommentText(e.target.value)}
                          onKeyDown={(e) => {
                            if (e.key === 'Enter' && commentText.trim()) {
                              addComment.mutate({ postId: post.id, content: commentText });
                            }
                          }}
                          className="flex-1 h-9 text-sm"
                        />
                        <Button
                          size="sm" className="h-9"
                          disabled={!commentText.trim() || addComment.isPending}
                          onClick={() => addComment.mutate({ postId: post.id, content: commentText })}
                        >
                          <Send className="h-3.5 w-3.5" />
                        </Button>
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </CardContent>
            </Card>
          </motion.div>
        ))}
      </AnimatePresence>

      {/* Empty state */}
      {!isLoading && posts.length === 0 && (
        <Card className="py-16 text-center">
          <CardContent>
            <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-muted flex items-center justify-center">
              <FileText className="h-8 w-8 text-muted-foreground" />
            </div>
            <h3 className="text-lg font-semibold mb-1">No posts yet</h3>
            <p className="text-muted-foreground text-sm">
              Start by sharing something with your network!
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
