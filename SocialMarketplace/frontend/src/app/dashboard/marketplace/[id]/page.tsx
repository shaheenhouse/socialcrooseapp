"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import Image from "next/image";
import Link from "next/link";
import { useParams } from "next/navigation";
import {
  Star,
  Heart,
  Share2,
  Clock,
  RefreshCw,
  Check,
  ChevronLeft,
  ChevronRight,
  MessageSquare,
  Shield,
  Award,
  MapPin,
  Calendar,
  Globe,
  ThumbsUp,
  Flag,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Progress } from "@/components/ui/progress";
import { Separator } from "@/components/ui/separator";

// Mock data - in real app, fetch from API
const service = {
  id: "1",
  title: "Professional Full-Stack Website Development",
  description: `Transform your business with a stunning, high-performance website built with the latest technologies. I specialize in creating modern, responsive web applications that deliver exceptional user experiences.

**What you'll get:**
- Custom website design tailored to your brand
- Responsive design that works on all devices
- Fast loading times optimized for SEO
- Clean, maintainable code
- Full documentation

**Technologies I use:**
- Frontend: React, Next.js, TypeScript, Tailwind CSS
- Backend: Node.js, Python, .NET Core
- Database: PostgreSQL, MongoDB, Redis
- Cloud: AWS, Vercel, Docker

Let's build something amazing together!`,
  seller: {
    id: "seller-1",
    name: "John Developer",
    username: "@johndev",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=John",
    level: "Top Rated",
    location: "Lahore, Pakistan",
    memberSince: "Jan 2021",
    languages: ["English", "Urdu"],
    responseTime: "1 hour",
    lastDelivery: "1 day",
    completedOrders: 234,
    rating: 4.9,
    reviews: 234,
    bio: "Full-stack developer with 8+ years of experience building scalable web applications. Passionate about clean code and great user experiences.",
    skills: ["React", "Next.js", "Node.js", "TypeScript", "PostgreSQL"],
  },
  images: [
    "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1498050108023-c5249f4df085?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1517180102446-f3ece451e9d8?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1504639725590-34d0984388bd?w=800&h=600&fit=crop",
  ],
  packages: [
    {
      name: "Basic",
      price: 500,
      description: "Perfect for small businesses",
      deliveryTime: 7,
      revisions: 2,
      features: [
        "5 pages",
        "Responsive design",
        "Contact form",
        "Basic SEO",
        "Social media integration",
      ],
    },
    {
      name: "Standard",
      price: 1000,
      description: "Most popular for growing businesses",
      deliveryTime: 14,
      revisions: 5,
      features: [
        "10 pages",
        "Responsive design",
        "Contact form",
        "Advanced SEO",
        "Social media integration",
        "CMS integration",
        "Analytics setup",
        "Performance optimization",
      ],
    },
    {
      name: "Premium",
      price: 2000,
      description: "Complete solution for enterprises",
      deliveryTime: 21,
      revisions: "Unlimited",
      features: [
        "Unlimited pages",
        "Responsive design",
        "Custom forms",
        "Full SEO package",
        "Social media integration",
        "Custom CMS",
        "Advanced analytics",
        "Performance optimization",
        "E-commerce ready",
        "API integrations",
        "Priority support",
      ],
    },
  ],
  category: "Web Development",
  tags: ["React", "Next.js", "Node.js", "TypeScript", "Full Stack"],
  reviews: [
    {
      id: "r1",
      user: {
        name: "Sarah Wilson",
        avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Sarah",
        country: "United States",
      },
      rating: 5,
      comment:
        "Absolutely amazing work! John delivered beyond my expectations. The website is fast, beautiful, and exactly what I needed. Highly recommend!",
      date: "2 weeks ago",
      helpful: 12,
    },
    {
      id: "r2",
      user: {
        name: "Mike Johnson",
        avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Mike",
        country: "Canada",
      },
      rating: 5,
      comment:
        "Professional, communicative, and delivered on time. The code quality is excellent and the documentation was thorough. Will work with again!",
      date: "1 month ago",
      helpful: 8,
    },
    {
      id: "r3",
      user: {
        name: "Emma Brown",
        avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Emma",
        country: "UK",
      },
      rating: 4,
      comment:
        "Great work overall. There were a few minor revisions needed, but John was quick to address them. Very satisfied with the result.",
      date: "1 month ago",
      helpful: 5,
    },
  ],
  ratingBreakdown: {
    5: 180,
    4: 40,
    3: 10,
    2: 3,
    1: 1,
  },
  faq: [
    {
      question: "What technologies do you use?",
      answer:
        "I primarily use React/Next.js for frontend, Node.js or .NET Core for backend, and PostgreSQL for databases. However, I can adapt to your specific requirements.",
    },
    {
      question: "Do you provide hosting setup?",
      answer:
        "Yes! I can help you set up hosting on platforms like Vercel, AWS, or any other provider you prefer. This is included in all packages.",
    },
    {
      question: "What about ongoing maintenance?",
      answer:
        "I offer maintenance packages separately. After project completion, we can discuss ongoing support and maintenance options.",
    },
  ],
};

export default function ServiceDetailPage() {
  const params = useParams();
  const [selectedPackage, setSelectedPackage] = useState(1);
  const [currentImage, setCurrentImage] = useState(0);
  const [liked, setLiked] = useState(false);

  const totalReviews = Object.values(service.ratingBreakdown).reduce(
    (a, b) => a + b,
    0
  );

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-2 text-sm text-muted-foreground">
        <Link href="/dashboard/marketplace" className="hover:text-foreground">
          Marketplace
        </Link>
        <span>/</span>
        <Link
          href={`/dashboard/marketplace?category=${service.category.toLowerCase()}`}
          className="hover:text-foreground"
        >
          {service.category}
        </Link>
        <span>/</span>
        <span className="text-foreground">{service.title}</span>
      </nav>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Main content */}
        <div className="space-y-6 lg:col-span-2">
          {/* Image gallery */}
          <Card className="overflow-hidden">
            <div className="relative aspect-video">
              <Image
                src={service.images[currentImage]}
                alt={service.title}
                fill
                className="object-cover"
              />
              <Button
                variant="ghost"
                size="icon"
                className="absolute left-2 top-1/2 -translate-y-1/2 bg-white/80 backdrop-blur-sm hover:bg-white"
                onClick={() =>
                  setCurrentImage((prev) =>
                    prev === 0 ? service.images.length - 1 : prev - 1
                  )
                }
              >
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="absolute right-2 top-1/2 -translate-y-1/2 bg-white/80 backdrop-blur-sm hover:bg-white"
                onClick={() =>
                  setCurrentImage((prev) =>
                    prev === service.images.length - 1 ? 0 : prev + 1
                  )
                }
              >
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
            <div className="flex gap-2 p-3">
              {service.images.map((image, index) => (
                <button
                  key={index}
                  onClick={() => setCurrentImage(index)}
                  className={cn(
                    "relative h-16 w-24 overflow-hidden rounded-md",
                    currentImage === index && "ring-2 ring-primary"
                  )}
                >
                  <Image
                    src={image}
                    alt={`Preview ${index + 1}`}
                    fill
                    className="object-cover"
                  />
                </button>
              ))}
            </div>
          </Card>

          {/* Title and actions */}
          <div className="flex items-start justify-between">
            <div>
              <h1 className="text-2xl font-bold">{service.title}</h1>
              <div className="mt-2 flex items-center gap-4">
                <div className="flex items-center gap-1">
                  <Star className="h-5 w-5 fill-amber-400 text-amber-400" />
                  <span className="font-semibold">{service.seller.rating}</span>
                  <span className="text-muted-foreground">
                    ({service.seller.reviews} reviews)
                  </span>
                </div>
                <span className="text-muted-foreground">
                  {service.seller.completedOrders} orders completed
                </span>
              </div>
            </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="icon"
                onClick={() => setLiked(!liked)}
              >
                <Heart
                  className={cn(
                    "h-4 w-4",
                    liked && "fill-red-500 text-red-500"
                  )}
                />
              </Button>
              <Button variant="outline" size="icon">
                <Share2 className="h-4 w-4" />
              </Button>
            </div>
          </div>

          {/* Tabs */}
          <Tabs defaultValue="description" className="w-full">
            <TabsList className="w-full justify-start">
              <TabsTrigger value="description">Description</TabsTrigger>
              <TabsTrigger value="reviews">Reviews</TabsTrigger>
              <TabsTrigger value="faq">FAQ</TabsTrigger>
            </TabsList>

            <TabsContent value="description" className="mt-4 space-y-4">
              <Card>
                <CardContent className="prose prose-sm max-w-none p-6 dark:prose-invert">
                  {service.description.split("\n").map((line, index) => {
                    if (line.startsWith("**") && line.endsWith("**")) {
                      return (
                        <h4 key={index} className="mt-4 font-semibold">
                          {line.replace(/\*\*/g, "")}
                        </h4>
                      );
                    }
                    if (line.startsWith("- ")) {
                      return (
                        <li key={index} className="ml-4">
                          {line.replace("- ", "")}
                        </li>
                      );
                    }
                    return line ? <p key={index}>{line}</p> : null;
                  })}
                </CardContent>
              </Card>

              {/* Tags */}
              <div className="flex flex-wrap gap-2">
                {service.tags.map((tag) => (
                  <Badge key={tag} variant="secondary">
                    {tag}
                  </Badge>
                ))}
              </div>
            </TabsContent>

            <TabsContent value="reviews" className="mt-4 space-y-6">
              {/* Rating summary */}
              <Card>
                <CardContent className="p-6">
                  <div className="flex flex-col gap-6 md:flex-row md:items-center">
                    <div className="text-center">
                      <div className="text-5xl font-bold">
                        {service.seller.rating}
                      </div>
                      <div className="mt-1 flex justify-center">
                        {[1, 2, 3, 4, 5].map((star) => (
                          <Star
                            key={star}
                            className={cn(
                              "h-5 w-5",
                              star <= Math.round(service.seller.rating)
                                ? "fill-amber-400 text-amber-400"
                                : "text-muted"
                            )}
                          />
                        ))}
                      </div>
                      <p className="mt-1 text-sm text-muted-foreground">
                        {totalReviews} reviews
                      </p>
                    </div>
                    <div className="flex-1 space-y-2">
                      {[5, 4, 3, 2, 1].map((rating) => (
                        <div key={rating} className="flex items-center gap-2">
                          <span className="w-12 text-sm">
                            {rating} stars
                          </span>
                          <Progress
                            value={
                              (service.ratingBreakdown[
                                rating as keyof typeof service.ratingBreakdown
                              ] /
                                totalReviews) *
                              100
                            }
                            className="h-2 flex-1"
                          />
                          <span className="w-12 text-right text-sm text-muted-foreground">
                            {
                              service.ratingBreakdown[
                                rating as keyof typeof service.ratingBreakdown
                              ]
                            }
                          </span>
                        </div>
                      ))}
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Reviews list */}
              <div className="space-y-4">
                {service.reviews.map((review) => (
                  <Card key={review.id}>
                    <CardContent className="p-6">
                      <div className="flex items-start justify-between">
                        <div className="flex items-center gap-3">
                          <Avatar>
                            <AvatarImage src={review.user.avatar} />
                            <AvatarFallback>
                              {review.user.name[0]}
                            </AvatarFallback>
                          </Avatar>
                          <div>
                            <p className="font-medium">{review.user.name}</p>
                            <p className="text-sm text-muted-foreground">
                              {review.user.country}
                            </p>
                          </div>
                        </div>
                        <div className="flex items-center gap-1">
                          {[1, 2, 3, 4, 5].map((star) => (
                            <Star
                              key={star}
                              className={cn(
                                "h-4 w-4",
                                star <= review.rating
                                  ? "fill-amber-400 text-amber-400"
                                  : "text-muted"
                              )}
                            />
                          ))}
                        </div>
                      </div>
                      <p className="mt-4">{review.comment}</p>
                      <div className="mt-4 flex items-center justify-between text-sm text-muted-foreground">
                        <span>{review.date}</span>
                        <div className="flex items-center gap-4">
                          <button className="flex items-center gap-1 hover:text-foreground">
                            <ThumbsUp className="h-4 w-4" />
                            Helpful ({review.helpful})
                          </button>
                          <button className="flex items-center gap-1 hover:text-foreground">
                            <Flag className="h-4 w-4" />
                            Report
                          </button>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </TabsContent>

            <TabsContent value="faq" className="mt-4 space-y-4">
              {service.faq.map((item, index) => (
                <Card key={index}>
                  <CardHeader>
                    <CardTitle className="text-base">{item.question}</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <p className="text-muted-foreground">{item.answer}</p>
                  </CardContent>
                </Card>
              ))}
            </TabsContent>
          </Tabs>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Package selector */}
          <Card className="sticky top-24">
            <CardHeader className="pb-4">
              <div className="flex gap-1">
                {service.packages.map((pkg, index) => (
                  <button
                    key={pkg.name}
                    onClick={() => setSelectedPackage(index)}
                    className={cn(
                      "flex-1 rounded-lg border-2 py-2 text-center text-sm font-medium transition-colors",
                      selectedPackage === index
                        ? "border-primary bg-primary text-primary-foreground"
                        : "border-border hover:border-primary"
                    )}
                  >
                    {pkg.name}
                  </button>
                ))}
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-baseline justify-between">
                <span className="text-3xl font-bold">
                  ${service.packages[selectedPackage].price}
                </span>
                <span className="text-muted-foreground">
                  {service.packages[selectedPackage].description}
                </span>
              </div>

              <div className="flex items-center gap-4 text-sm text-muted-foreground">
                <div className="flex items-center gap-1">
                  <Clock className="h-4 w-4" />
                  {service.packages[selectedPackage].deliveryTime} days delivery
                </div>
                <div className="flex items-center gap-1">
                  <RefreshCw className="h-4 w-4" />
                  {service.packages[selectedPackage].revisions} revisions
                </div>
              </div>

              <Separator />

              <ul className="space-y-2">
                {service.packages[selectedPackage].features.map(
                  (feature, index) => (
                    <li key={index} className="flex items-center gap-2 text-sm">
                      <Check className="h-4 w-4 text-green-500" />
                      {feature}
                    </li>
                  )
                )}
              </ul>

              <div className="space-y-2 pt-4">
                <Button className="w-full" size="lg">
                  Continue (${service.packages[selectedPackage].price})
                </Button>
                <Button variant="outline" className="w-full" size="lg">
                  <MessageSquare className="mr-2 h-4 w-4" />
                  Contact Seller
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Seller card */}
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center gap-4">
                <Avatar className="h-16 w-16">
                  <AvatarImage src={service.seller.avatar} />
                  <AvatarFallback>{service.seller.name[0]}</AvatarFallback>
                </Avatar>
                <div>
                  <h3 className="font-semibold">{service.seller.name}</h3>
                  <p className="text-sm text-muted-foreground">
                    {service.seller.username}
                  </p>
                  <Badge variant="secondary" className="mt-1">
                    <Award className="mr-1 h-3 w-3" />
                    {service.seller.level}
                  </Badge>
                </div>
              </div>

              <Link href={`/dashboard/profile/${service.seller.id}`}>
                <Button variant="outline" className="mt-4 w-full">
                  View Profile
                </Button>
              </Link>

              <Separator className="my-4" />

              <div className="space-y-3 text-sm">
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">From</span>
                  <div className="flex items-center gap-1">
                    <MapPin className="h-4 w-4" />
                    {service.seller.location}
                  </div>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Member since</span>
                  <div className="flex items-center gap-1">
                    <Calendar className="h-4 w-4" />
                    {service.seller.memberSince}
                  </div>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Avg. response</span>
                  <span>{service.seller.responseTime}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Last delivery</span>
                  <span>{service.seller.lastDelivery}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Languages</span>
                  <div className="flex items-center gap-1">
                    <Globe className="h-4 w-4" />
                    {service.seller.languages.join(", ")}
                  </div>
                </div>
              </div>

              <Separator className="my-4" />

              <p className="text-sm text-muted-foreground">
                {service.seller.bio}
              </p>

              <div className="mt-4 flex flex-wrap gap-1">
                {service.seller.skills.map((skill) => (
                  <Badge key={skill} variant="outline" className="text-xs">
                    {skill}
                  </Badge>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Trust badges */}
          <Card>
            <CardContent className="p-6">
              <div className="space-y-4">
                <div className="flex items-center gap-3">
                  <div className="rounded-full bg-green-500/10 p-2">
                    <Shield className="h-5 w-5 text-green-500" />
                  </div>
                  <div>
                    <p className="font-medium">Secure Payment</p>
                    <p className="text-sm text-muted-foreground">
                      Money held in escrow until delivery
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <div className="rounded-full bg-blue-500/10 p-2">
                    <Award className="h-5 w-5 text-blue-500" />
                  </div>
                  <div>
                    <p className="font-medium">Quality Guaranteed</p>
                    <p className="text-sm text-muted-foreground">
                      100% satisfaction or money back
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <div className="rounded-full bg-purple-500/10 p-2">
                    <MessageSquare className="h-5 w-5 text-purple-500" />
                  </div>
                  <div>
                    <p className="font-medium">24/7 Support</p>
                    <p className="text-sm text-muted-foreground">
                      Get help whenever you need it
                    </p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
