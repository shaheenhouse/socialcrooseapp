"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import Image from "next/image";
import {
  User,
  Mail,
  Phone,
  MapPin,
  Globe,
  Calendar,
  Star,
  Award,
  Briefcase,
  Edit,
  Camera,
  Shield,
  CheckCircle2,
  ExternalLink,
  Github,
  Linkedin,
  Twitter,
  Plus,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Progress } from "@/components/ui/progress";
import { Separator } from "@/components/ui/separator";
import { useAuthStore } from "@/store/auth-store";

const profileData = {
  firstName: "Ahmed",
  lastName: "Khan",
  username: "@ahmedkhan",
  email: "ahmed@example.com",
  phone: "+92 300 1234567",
  location: "Lahore, Pakistan",
  timezone: "PKT (UTC+5)",
  memberSince: "January 2023",
  bio: "Full-stack developer with 5+ years of experience in building scalable web applications. Passionate about clean code, user experience, and helping businesses grow through technology.",
  avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Ahmed",
  coverImage: "https://images.unsplash.com/photo-1579547621113-e4bb2a19bdd6?w=1200&h=400&fit=crop",
  verified: true,
  level: "Top Rated",
  completionRate: 98,
  responseTime: "1 hour",
  languages: ["English", "Urdu", "Punjabi"],
  stats: {
    completedProjects: 156,
    totalEarnings: 2450000,
    avgRating: 4.9,
    totalReviews: 142,
    repeatClients: 45,
  },
  skills: [
    { name: "React", level: 95, verified: true },
    { name: "Next.js", level: 92, verified: true },
    { name: "Node.js", level: 88, verified: true },
    { name: "TypeScript", level: 90, verified: false },
    { name: "PostgreSQL", level: 85, verified: true },
    { name: "Python", level: 75, verified: false },
  ],
  education: [
    {
      degree: "BS Computer Science",
      institution: "LUMS",
      year: "2018",
    },
    {
      degree: "AWS Certified Developer",
      institution: "Amazon Web Services",
      year: "2022",
    },
  ],
  experience: [
    {
      title: "Senior Full-Stack Developer",
      company: "Tech Corp",
      period: "2020 - Present",
      description: "Leading development of enterprise applications",
    },
    {
      title: "Full-Stack Developer",
      company: "StartUp Inc",
      period: "2018 - 2020",
      description: "Built multiple web and mobile applications",
    },
  ],
  portfolio: [
    {
      id: "p1",
      title: "E-commerce Platform",
      image: "https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=400&h=300&fit=crop",
      category: "Web Development",
    },
    {
      id: "p2",
      title: "Healthcare Dashboard",
      image: "https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?w=400&h=300&fit=crop",
      category: "Dashboard",
    },
    {
      id: "p3",
      title: "Mobile Banking App",
      image: "https://images.unsplash.com/photo-1563986768494-4dee2763ff3f?w=400&h=300&fit=crop",
      category: "Mobile App",
    },
  ],
  socialLinks: {
    website: "https://ahmedkhan.dev",
    github: "ahmedkhan",
    linkedin: "ahmedkhan",
    twitter: "ahmedkhan_dev",
  },
};

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: {
    opacity: 1,
    y: 0,
    transition: { duration: 0.4 },
  },
};

function formatCurrency(amount: number) {
  if (amount >= 1000000) {
    return `${(amount / 1000000).toFixed(1)}M`;
  }
  return `${(amount / 1000).toFixed(0)}K`;
}

export default function ProfilePage() {
  const { user } = useAuthStore();

  return (
    <div className="space-y-6">
      {/* Cover image and profile header */}
      <Card className="overflow-hidden">
        <div className="relative h-48 w-full">
          <Image
            src={profileData.coverImage}
            alt="Cover"
            fill
            className="object-cover"
          />
          <Button
            variant="secondary"
            size="sm"
            className="absolute right-4 top-4"
          >
            <Camera className="mr-2 h-4 w-4" />
            Change Cover
          </Button>
        </div>
        <CardContent className="relative pb-6">
          <div className="flex flex-col items-center gap-4 sm:flex-row sm:items-end">
            <div className="relative -mt-16">
              <Avatar className="h-32 w-32 border-4 border-background">
                <AvatarImage src={profileData.avatar} />
                <AvatarFallback>
                  {profileData.firstName[0]}
                  {profileData.lastName[0]}
                </AvatarFallback>
              </Avatar>
              <Button
                variant="secondary"
                size="icon"
                className="absolute bottom-0 right-0"
              >
                <Camera className="h-4 w-4" />
              </Button>
            </div>
            <div className="flex-1 text-center sm:text-left">
              <div className="flex items-center justify-center gap-2 sm:justify-start">
                <h1 className="text-2xl font-bold">
                  {profileData.firstName} {profileData.lastName}
                </h1>
                {profileData.verified && (
                  <CheckCircle2 className="h-5 w-5 text-blue-500" />
                )}
                <Badge variant="secondary">
                  <Award className="mr-1 h-3 w-3" />
                  {profileData.level}
                </Badge>
              </div>
              <p className="text-muted-foreground">{profileData.username}</p>
              <div className="mt-2 flex flex-wrap items-center justify-center gap-4 text-sm text-muted-foreground sm:justify-start">
                <span className="flex items-center gap-1">
                  <MapPin className="h-4 w-4" />
                  {profileData.location}
                </span>
                <span className="flex items-center gap-1">
                  <Calendar className="h-4 w-4" />
                  Member since {profileData.memberSince}
                </span>
              </div>
            </div>
            <Button>
              <Edit className="mr-2 h-4 w-4" />
              Edit Profile
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-5">
        <Card>
          <CardContent className="p-4 text-center">
            <p className="text-3xl font-bold">{profileData.stats.completedProjects}</p>
            <p className="text-sm text-muted-foreground">Projects</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <p className="text-3xl font-bold">
              {formatCurrency(profileData.stats.totalEarnings)}
            </p>
            <p className="text-sm text-muted-foreground">Total Earnings</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <div className="flex items-center justify-center gap-1">
              <Star className="h-5 w-5 fill-amber-400 text-amber-400" />
              <span className="text-3xl font-bold">{profileData.stats.avgRating}</span>
            </div>
            <p className="text-sm text-muted-foreground">
              {profileData.stats.totalReviews} reviews
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <p className="text-3xl font-bold">{profileData.completionRate}%</p>
            <p className="text-sm text-muted-foreground">Completion Rate</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 text-center">
            <p className="text-3xl font-bold">{profileData.stats.repeatClients}</p>
            <p className="text-sm text-muted-foreground">Repeat Clients</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Main content */}
        <div className="space-y-6 lg:col-span-2">
          <Tabs defaultValue="overview" className="w-full">
            <TabsList>
              <TabsTrigger value="overview">Overview</TabsTrigger>
              <TabsTrigger value="portfolio">Portfolio</TabsTrigger>
              <TabsTrigger value="reviews">Reviews</TabsTrigger>
            </TabsList>

            <TabsContent value="overview" className="mt-6 space-y-6">
              {/* Bio */}
              <Card>
                <CardHeader>
                  <CardTitle>About</CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-muted-foreground">{profileData.bio}</p>
                </CardContent>
              </Card>

              {/* Skills */}
              <Card>
                <CardHeader className="flex flex-row items-center justify-between">
                  <CardTitle>Skills</CardTitle>
                  <Button variant="outline" size="sm">
                    <Plus className="mr-1 h-4 w-4" />
                    Add Skill
                  </Button>
                </CardHeader>
                <CardContent className="space-y-4">
                  {profileData.skills.map((skill) => (
                    <div key={skill.name} className="space-y-2">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <span className="font-medium">{skill.name}</span>
                          {skill.verified && (
                            <Badge variant="outline" className="text-xs">
                              <Shield className="mr-1 h-3 w-3 text-green-500" />
                              Verified
                            </Badge>
                          )}
                        </div>
                        <span className="text-sm text-muted-foreground">
                          {skill.level}%
                        </span>
                      </div>
                      <Progress value={skill.level} className="h-2" />
                    </div>
                  ))}
                </CardContent>
              </Card>

              {/* Experience */}
              <Card>
                <CardHeader className="flex flex-row items-center justify-between">
                  <CardTitle>Experience</CardTitle>
                  <Button variant="outline" size="sm">
                    <Plus className="mr-1 h-4 w-4" />
                    Add
                  </Button>
                </CardHeader>
                <CardContent className="space-y-4">
                  {profileData.experience.map((exp, index) => (
                    <div key={index} className="flex gap-4">
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10">
                        <Briefcase className="h-5 w-5 text-primary" />
                      </div>
                      <div>
                        <h4 className="font-semibold">{exp.title}</h4>
                        <p className="text-sm text-muted-foreground">
                          {exp.company} • {exp.period}
                        </p>
                        <p className="mt-1 text-sm text-muted-foreground">
                          {exp.description}
                        </p>
                      </div>
                    </div>
                  ))}
                </CardContent>
              </Card>

              {/* Education */}
              <Card>
                <CardHeader className="flex flex-row items-center justify-between">
                  <CardTitle>Education & Certifications</CardTitle>
                  <Button variant="outline" size="sm">
                    <Plus className="mr-1 h-4 w-4" />
                    Add
                  </Button>
                </CardHeader>
                <CardContent className="space-y-4">
                  {profileData.education.map((edu, index) => (
                    <div key={index} className="flex gap-4">
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10">
                        <Award className="h-5 w-5 text-primary" />
                      </div>
                      <div>
                        <h4 className="font-semibold">{edu.degree}</h4>
                        <p className="text-sm text-muted-foreground">
                          {edu.institution} • {edu.year}
                        </p>
                      </div>
                    </div>
                  ))}
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="portfolio" className="mt-6">
              <motion.div
                variants={containerVariants}
                initial="hidden"
                animate="visible"
                className="grid gap-4 sm:grid-cols-2"
              >
                {profileData.portfolio.map((item) => (
                  <motion.div key={item.id} variants={itemVariants}>
                    <Card className="group cursor-pointer overflow-hidden transition-all hover:shadow-lg">
                      <div className="relative aspect-video">
                        <Image
                          src={item.image}
                          alt={item.title}
                          fill
                          className="object-cover transition-transform group-hover:scale-105"
                        />
                        <div className="absolute inset-0 flex items-center justify-center bg-black/50 opacity-0 transition-opacity group-hover:opacity-100">
                          <Button variant="secondary">
                            <ExternalLink className="mr-2 h-4 w-4" />
                            View Project
                          </Button>
                        </div>
                      </div>
                      <CardContent className="p-4">
                        <h3 className="font-semibold">{item.title}</h3>
                        <p className="text-sm text-muted-foreground">
                          {item.category}
                        </p>
                      </CardContent>
                    </Card>
                  </motion.div>
                ))}
                <Card className="flex aspect-video cursor-pointer items-center justify-center border-dashed transition-all hover:border-primary hover:bg-muted/50">
                  <div className="text-center">
                    <Plus className="mx-auto h-8 w-8 text-muted-foreground" />
                    <p className="mt-2 font-medium">Add New Project</p>
                  </div>
                </Card>
              </motion.div>
            </TabsContent>

            <TabsContent value="reviews" className="mt-6">
              <Card>
                <CardContent className="flex flex-col items-center justify-center py-12">
                  <Star className="h-12 w-12 text-muted-foreground/50" />
                  <h3 className="mt-4 text-lg font-semibold">Reviews Coming Soon</h3>
                  <p className="text-muted-foreground">
                    Your client reviews will appear here
                  </p>
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Contact info */}
          <Card>
            <CardHeader>
              <CardTitle>Contact Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center gap-3">
                <Mail className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm">{profileData.email}</span>
              </div>
              <div className="flex items-center gap-3">
                <Phone className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm">{profileData.phone}</span>
              </div>
              <div className="flex items-center gap-3">
                <Globe className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm">{profileData.timezone}</span>
              </div>

              <Separator />

              <div className="flex items-center gap-3">
                <Globe className="h-4 w-4 text-muted-foreground" />
                <a
                  href={profileData.socialLinks.website}
                  className="text-sm text-primary hover:underline"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  {profileData.socialLinks.website}
                </a>
              </div>
              <div className="flex gap-2">
                <Button variant="outline" size="icon">
                  <Github className="h-4 w-4" />
                </Button>
                <Button variant="outline" size="icon">
                  <Linkedin className="h-4 w-4" />
                </Button>
                <Button variant="outline" size="icon">
                  <Twitter className="h-4 w-4" />
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Languages */}
          <Card>
            <CardHeader>
              <CardTitle>Languages</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {profileData.languages.map((lang) => (
                  <Badge key={lang} variant="secondary">
                    {lang}
                  </Badge>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Response time */}
          <Card>
            <CardHeader>
              <CardTitle>Response Time</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-2">
                <div className="h-2 w-2 rounded-full bg-green-500" />
                <span className="font-medium">{profileData.responseTime}</span>
              </div>
              <p className="mt-1 text-sm text-muted-foreground">
                Average response time
              </p>
            </CardContent>
          </Card>

          {/* Verification */}
          <Card>
            <CardHeader>
              <CardTitle>Verification</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-sm">Identity</span>
                <CheckCircle2 className="h-5 w-5 text-green-500" />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm">Email</span>
                <CheckCircle2 className="h-5 w-5 text-green-500" />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm">Phone</span>
                <CheckCircle2 className="h-5 w-5 text-green-500" />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm">Payment Method</span>
                <CheckCircle2 className="h-5 w-5 text-green-500" />
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
