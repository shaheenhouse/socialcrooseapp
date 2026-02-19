"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import Link from "next/link";
import {
  Search,
  Plus,
  Filter,
  Clock,
  DollarSign,
  Users,
  Calendar,
  ChevronDown,
  Star,
  Eye,
  MessageSquare,
  Briefcase,
  Target,
  MapPin,
  CheckCircle2,
  AlertCircle,
  ArrowUpRight,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Progress } from "@/components/ui/progress";

const categories = [
  "All",
  "Web Development",
  "Mobile Apps",
  "UI/UX Design",
  "Data Science",
  "Marketing",
  "Content Writing",
];

const projects = [
  {
    id: "p1",
    title: "E-commerce Platform Development",
    description:
      "Looking for an experienced developer to build a complete e-commerce platform with React frontend and Node.js backend. Must include payment integration, inventory management, and admin dashboard.",
    budget: { min: 5000, max: 10000 },
    bids: 12,
    deadline: "Dec 30, 2025",
    skills: ["React", "Node.js", "PostgreSQL", "Stripe"],
    client: {
      name: "Tech Solutions Inc",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=TS",
      rating: 4.8,
      totalSpent: 45000,
      projects: 23,
      verified: true,
    },
    postedAt: "2 days ago",
    category: "Web Development",
    experienceLevel: "Expert",
    projectLength: "3-6 months",
    location: "Remote",
  },
  {
    id: "p2",
    title: "Mobile App UI/UX Redesign",
    description:
      "Need a talented UI/UX designer to redesign our existing fitness mobile app. Focus on improving user experience, modern design aesthetics, and accessibility. Figma experience required.",
    budget: { min: 2000, max: 4000 },
    bids: 28,
    deadline: "Jan 15, 2026",
    skills: ["Figma", "UI/UX", "Mobile Design", "Prototyping"],
    client: {
      name: "FitLife App",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=FL",
      rating: 4.9,
      totalSpent: 32000,
      projects: 15,
      verified: true,
    },
    postedAt: "1 day ago",
    category: "UI/UX Design",
    experienceLevel: "Intermediate",
    projectLength: "1-3 months",
    location: "Remote",
  },
  {
    id: "p3",
    title: "Machine Learning Model for Sales Prediction",
    description:
      "Looking for a data scientist to build a machine learning model that predicts sales based on historical data. Must include data preprocessing, model training, and deployment pipeline.",
    budget: { min: 8000, max: 15000 },
    bids: 8,
    deadline: "Feb 1, 2026",
    skills: ["Python", "TensorFlow", "Pandas", "AWS"],
    client: {
      name: "DataDriven Co",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=DD",
      rating: 4.7,
      totalSpent: 78000,
      projects: 42,
      verified: true,
    },
    postedAt: "5 hours ago",
    category: "Data Science",
    experienceLevel: "Expert",
    projectLength: "1-3 months",
    location: "Remote",
  },
  {
    id: "p4",
    title: "WordPress Blog Setup & SEO Optimization",
    description:
      "Need someone to set up a professional WordPress blog with custom theme, SEO optimization, and content migration from our old website. Speed optimization is critical.",
    budget: { min: 500, max: 1500 },
    bids: 35,
    deadline: "Dec 20, 2025",
    skills: ["WordPress", "SEO", "PHP", "Content Migration"],
    client: {
      name: "Digital Media Pro",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=DM",
      rating: 4.5,
      totalSpent: 12000,
      projects: 8,
      verified: false,
    },
    postedAt: "3 days ago",
    category: "Web Development",
    experienceLevel: "Intermediate",
    projectLength: "Less than 1 month",
    location: "Remote",
  },
  {
    id: "p5",
    title: "Social Media Marketing Campaign",
    description:
      "Seeking a digital marketing expert to run a comprehensive social media campaign across Instagram, TikTok, and Facebook. Must include content creation, scheduling, and analytics reporting.",
    budget: { min: 3000, max: 6000 },
    bids: 19,
    deadline: "Jan 31, 2026",
    skills: ["Social Media", "Content Marketing", "Analytics", "Ads"],
    client: {
      name: "StyleBrand Fashion",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=SB",
      rating: 4.6,
      totalSpent: 28000,
      projects: 12,
      verified: true,
    },
    postedAt: "12 hours ago",
    category: "Marketing",
    experienceLevel: "Intermediate",
    projectLength: "3-6 months",
    location: "Remote",
  },
];

const myProjects = [
  {
    id: "mp1",
    title: "Healthcare Dashboard Development",
    client: {
      name: "MediCare Plus",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=MC",
    },
    budget: 8500,
    progress: 65,
    deadline: "Jan 10, 2026",
    status: "in-progress",
    milestones: { completed: 3, total: 5 },
  },
  {
    id: "mp2",
    title: "Restaurant Booking App",
    client: {
      name: "FoodHub",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=FH",
    },
    budget: 4200,
    progress: 90,
    deadline: "Dec 15, 2025",
    status: "review",
    milestones: { completed: 4, total: 4 },
  },
  {
    id: "mp3",
    title: "Inventory Management System",
    client: {
      name: "RetailMax",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=RM",
    },
    budget: 6800,
    progress: 25,
    deadline: "Feb 28, 2026",
    status: "in-progress",
    milestones: { completed: 1, total: 6 },
  },
];

const statusColors: Record<string, string> = {
  "in-progress": "bg-blue-500/10 text-blue-500 border-blue-500/20",
  review: "bg-amber-500/10 text-amber-500 border-amber-500/20",
  completed: "bg-green-500/10 text-green-500 border-green-500/20",
  pending: "bg-gray-500/10 text-gray-500 border-gray-500/20",
};

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.1,
    },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: {
    opacity: 1,
    y: 0,
    transition: {
      duration: 0.4,
    },
  },
};

export default function ProjectsPage() {
  const [selectedCategory, setSelectedCategory] = useState("All");
  const [searchQuery, setSearchQuery] = useState("");

  const filteredProjects = projects.filter((project) => {
    if (
      selectedCategory !== "All" &&
      project.category !== selectedCategory
    ) {
      return false;
    }
    if (
      searchQuery &&
      !project.title.toLowerCase().includes(searchQuery.toLowerCase())
    ) {
      return false;
    }
    return true;
  });

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold">Projects</h1>
          <p className="text-muted-foreground">
            Find and manage your projects
          </p>
        </div>
        <Button>
          <Plus className="mr-2 h-4 w-4" />
          Post a Project
        </Button>
      </div>

      <Tabs defaultValue="browse" className="w-full">
        <TabsList>
          <TabsTrigger value="browse">Browse Projects</TabsTrigger>
          <TabsTrigger value="my-projects">My Projects</TabsTrigger>
          <TabsTrigger value="my-bids">My Bids</TabsTrigger>
        </TabsList>

        {/* Browse Projects Tab */}
        <TabsContent value="browse" className="mt-6 space-y-6">
          {/* Search and filters */}
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div className="relative flex-1 lg:max-w-md">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search projects..."
                className="pl-9"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
            <div className="flex flex-wrap gap-2">
              {categories.map((category) => (
                <Button
                  key={category}
                  variant={selectedCategory === category ? "default" : "outline"}
                  size="sm"
                  onClick={() => setSelectedCategory(category)}
                >
                  {category}
                </Button>
              ))}
            </div>
          </div>

          {/* Projects list */}
          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className="space-y-4"
          >
            {filteredProjects.map((project) => (
              <motion.div key={project.id} variants={itemVariants}>
                <Card className="transition-all hover:shadow-lg">
                  <CardContent className="p-6">
                    <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                      <div className="flex-1 space-y-3">
                        <div className="flex items-start justify-between">
                          <div>
                            <Link
                              href={`/dashboard/projects/${project.id}`}
                              className="text-xl font-semibold hover:text-primary"
                            >
                              {project.title}
                            </Link>
                            <div className="mt-1 flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
                              <span>{project.category}</span>
                              <span>•</span>
                              <span>Posted {project.postedAt}</span>
                              <span>•</span>
                              <span className="flex items-center gap-1">
                                <MapPin className="h-3 w-3" />
                                {project.location}
                              </span>
                            </div>
                          </div>
                          <Badge variant="outline">{project.experienceLevel}</Badge>
                        </div>

                        <p className="line-clamp-2 text-muted-foreground">
                          {project.description}
                        </p>

                        <div className="flex flex-wrap gap-1">
                          {project.skills.map((skill) => (
                            <Badge key={skill} variant="secondary" className="text-xs">
                              {skill}
                            </Badge>
                          ))}
                        </div>

                        <div className="flex flex-wrap items-center gap-4 text-sm">
                          <div className="flex items-center gap-1">
                            <DollarSign className="h-4 w-4 text-green-500" />
                            <span className="font-semibold">
                              ${project.budget.min.toLocaleString()} - $
                              {project.budget.max.toLocaleString()}
                            </span>
                          </div>
                          <div className="flex items-center gap-1 text-muted-foreground">
                            <Users className="h-4 w-4" />
                            {project.bids} bids
                          </div>
                          <div className="flex items-center gap-1 text-muted-foreground">
                            <Calendar className="h-4 w-4" />
                            Due: {project.deadline}
                          </div>
                          <div className="flex items-center gap-1 text-muted-foreground">
                            <Clock className="h-4 w-4" />
                            {project.projectLength}
                          </div>
                        </div>
                      </div>

                      {/* Client info */}
                      <div className="flex items-center gap-3 rounded-lg border p-4 lg:ml-6 lg:w-72">
                        <Avatar>
                          <AvatarImage src={project.client.avatar} />
                          <AvatarFallback>
                            {project.client.name[0]}
                          </AvatarFallback>
                        </Avatar>
                        <div className="flex-1">
                          <div className="flex items-center gap-2">
                            <span className="font-medium">{project.client.name}</span>
                            {project.client.verified && (
                              <CheckCircle2 className="h-4 w-4 text-blue-500" />
                            )}
                          </div>
                          <div className="flex items-center gap-2 text-sm text-muted-foreground">
                            <Star className="h-3 w-3 fill-amber-400 text-amber-400" />
                            {project.client.rating}
                            <span>•</span>
                            ${(project.client.totalSpent / 1000).toFixed(0)}k spent
                          </div>
                          <span className="text-xs text-muted-foreground">
                            {project.client.projects} projects
                          </span>
                        </div>
                      </div>
                    </div>

                    <div className="mt-4 flex items-center justify-between border-t pt-4">
                      <div className="flex items-center gap-2">
                        <Button variant="ghost" size="sm">
                          <Eye className="mr-1 h-4 w-4" />
                          View Details
                        </Button>
                        <Button variant="ghost" size="sm">
                          <MessageSquare className="mr-1 h-4 w-4" />
                          Ask Question
                        </Button>
                      </div>
                      <Button>
                        Submit Proposal
                        <ArrowUpRight className="ml-2 h-4 w-4" />
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              </motion.div>
            ))}
          </motion.div>
        </TabsContent>

        {/* My Projects Tab */}
        <TabsContent value="my-projects" className="mt-6 space-y-6">
          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className="grid gap-4 md:grid-cols-2 lg:grid-cols-3"
          >
            {myProjects.map((project) => (
              <motion.div key={project.id} variants={itemVariants}>
                <Card className="transition-all hover:shadow-lg">
                  <CardHeader>
                    <div className="flex items-start justify-between">
                      <Badge
                        variant="outline"
                        className={statusColors[project.status]}
                      >
                        {project.status === "in-progress"
                          ? "In Progress"
                          : project.status === "review"
                          ? "In Review"
                          : project.status}
                      </Badge>
                      <span className="text-sm text-muted-foreground">
                        Due: {project.deadline}
                      </span>
                    </div>
                    <CardTitle className="line-clamp-2">{project.title}</CardTitle>
                    <CardDescription>
                      <div className="flex items-center gap-2">
                        <Avatar className="h-6 w-6">
                          <AvatarImage src={project.client.avatar} />
                          <AvatarFallback>{project.client.name[0]}</AvatarFallback>
                        </Avatar>
                        {project.client.name}
                      </div>
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Progress</span>
                      <span className="font-medium">{project.progress}%</span>
                    </div>
                    <Progress value={project.progress} className="h-2" />

                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Milestones</span>
                      <span>
                        {project.milestones.completed} / {project.milestones.total}
                      </span>
                    </div>

                    <div className="flex items-center justify-between">
                      <span className="text-sm text-muted-foreground">Budget</span>
                      <span className="text-lg font-bold">
                        ${project.budget.toLocaleString()}
                      </span>
                    </div>
                  </CardContent>
                  <CardFooter>
                    <Button variant="outline" className="w-full">
                      View Project
                    </Button>
                  </CardFooter>
                </Card>
              </motion.div>
            ))}

            {/* Add new project card */}
            <motion.div variants={itemVariants}>
              <Card className="flex h-full min-h-[300px] cursor-pointer items-center justify-center border-dashed transition-all hover:border-primary hover:bg-muted/50">
                <div className="text-center">
                  <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-primary/10">
                    <Plus className="h-6 w-6 text-primary" />
                  </div>
                  <h3 className="font-semibold">Start New Project</h3>
                  <p className="mt-1 text-sm text-muted-foreground">
                    Create a new project listing
                  </p>
                </div>
              </Card>
            </motion.div>
          </motion.div>
        </TabsContent>

        {/* My Bids Tab */}
        <TabsContent value="my-bids" className="mt-6">
          <Card>
            <CardHeader>
              <CardTitle>Your Active Bids</CardTitle>
              <CardDescription>
                Track the status of your project proposals
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {[1, 2, 3].map((i) => (
                  <div
                    key={i}
                    className="flex items-center justify-between rounded-lg border p-4"
                  >
                    <div className="flex items-center gap-4">
                      <div className="rounded-full bg-primary/10 p-2">
                        <Briefcase className="h-5 w-5 text-primary" />
                      </div>
                      <div>
                        <p className="font-medium">Project Title #{i}</p>
                        <p className="text-sm text-muted-foreground">
                          Bid submitted 2 days ago
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-4">
                      <div className="text-right">
                        <p className="font-semibold">$2,500</p>
                        <p className="text-sm text-muted-foreground">Your bid</p>
                      </div>
                      <Badge
                        variant="outline"
                        className={
                          i === 1
                            ? "bg-amber-500/10 text-amber-500"
                            : i === 2
                            ? "bg-green-500/10 text-green-500"
                            : "bg-gray-500/10 text-gray-500"
                        }
                      >
                        {i === 1 ? "Pending" : i === 2 ? "Accepted" : "Declined"}
                      </Badge>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
