"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import Image from "next/image";
import Link from "next/link";
import {
  Award,
  Clock,
  CheckCircle2,
  XCircle,
  Play,
  Lock,
  ChevronRight,
  Star,
  Trophy,
  Target,
  BookOpen,
  FileText,
  Calendar,
  Timer,
  BarChart3,
  TrendingUp,
  Shield,
  Zap,
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
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

const skillCategories = [
  { id: "all", name: "All Skills" },
  { id: "programming", name: "Programming" },
  { id: "design", name: "Design" },
  { id: "marketing", name: "Marketing" },
  { id: "data", name: "Data Science" },
  { id: "management", name: "Project Management" },
];

const availableTests = [
  {
    id: "t1",
    name: "JavaScript Advanced",
    category: "programming",
    questions: 50,
    duration: 60,
    difficulty: "Advanced",
    description: "Test your advanced JavaScript knowledge including ES6+, async patterns, and more",
    takers: 15234,
    passRate: 62,
    badge: "JavaScript Expert",
    icon: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/javascript/javascript-original.svg",
  },
  {
    id: "t2",
    name: "React Development",
    category: "programming",
    questions: 40,
    duration: 45,
    difficulty: "Intermediate",
    description: "Comprehensive test covering React hooks, state management, and best practices",
    takers: 12567,
    passRate: 58,
    badge: "React Developer",
    icon: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/react/react-original.svg",
  },
  {
    id: "t3",
    name: "UI/UX Design Principles",
    category: "design",
    questions: 35,
    duration: 40,
    difficulty: "Intermediate",
    description: "Test your understanding of user interface and user experience design fundamentals",
    takers: 8934,
    passRate: 71,
    badge: "UX Designer",
    icon: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/figma/figma-original.svg",
  },
  {
    id: "t4",
    name: "Python for Data Science",
    category: "data",
    questions: 45,
    duration: 55,
    difficulty: "Advanced",
    description: "Covers Python libraries like Pandas, NumPy, and machine learning basics",
    takers: 9876,
    passRate: 54,
    badge: "Data Scientist",
    icon: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/python/python-original.svg",
  },
  {
    id: "t5",
    name: "Digital Marketing",
    category: "marketing",
    questions: 30,
    duration: 35,
    difficulty: "Beginner",
    description: "Fundamentals of digital marketing including SEO, social media, and analytics",
    takers: 11234,
    passRate: 78,
    badge: "Marketing Pro",
    icon: "📊",
  },
  {
    id: "t6",
    name: "Agile & Scrum",
    category: "management",
    questions: 25,
    duration: 30,
    difficulty: "Intermediate",
    description: "Test your knowledge of Agile methodologies and Scrum framework",
    takers: 7654,
    passRate: 82,
    badge: "Scrum Master",
    icon: "📋",
  },
];

const myCertificates = [
  {
    id: "c1",
    name: "JavaScript Expert",
    issuedDate: "Oct 15, 2025",
    expiryDate: "Oct 15, 2027",
    score: 92,
    status: "active",
    credentialId: "JS-2025-78234",
    icon: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/javascript/javascript-original.svg",
  },
  {
    id: "c2",
    name: "React Developer",
    issuedDate: "Sep 20, 2025",
    expiryDate: "Sep 20, 2027",
    score: 88,
    status: "active",
    credentialId: "RCT-2025-45123",
    icon: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/react/react-original.svg",
  },
  {
    id: "c3",
    name: "Python Basics",
    issuedDate: "Mar 10, 2024",
    expiryDate: "Mar 10, 2026",
    score: 85,
    status: "expiring",
    credentialId: "PY-2024-12456",
    icon: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/python/python-original.svg",
  },
];

const skillProgress = [
  { name: "JavaScript", level: 92, verified: true },
  { name: "React", level: 88, verified: true },
  { name: "TypeScript", level: 78, verified: false },
  { name: "Node.js", level: 75, verified: false },
  { name: "Python", level: 65, verified: true },
];

const difficultyColors: Record<string, string> = {
  Beginner: "bg-green-500/10 text-green-500",
  Intermediate: "bg-amber-500/10 text-amber-500",
  Advanced: "bg-red-500/10 text-red-500",
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

export default function SkillsPage() {
  const [selectedCategory, setSelectedCategory] = useState("all");

  const filteredTests = availableTests.filter(
    (test) => selectedCategory === "all" || test.category === selectedCategory
  );

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold">Skills & Certifications</h1>
          <p className="text-muted-foreground">
            Take tests to verify your skills and earn certificates
          </p>
        </div>
      </div>

      {/* Stats cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-primary/10 p-3">
              <Award className="h-6 w-6 text-primary" />
            </div>
            <div>
              <p className="text-2xl font-bold">3</p>
              <p className="text-sm text-muted-foreground">Certificates</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-green-500/10 p-3">
              <CheckCircle2 className="h-6 w-6 text-green-500" />
            </div>
            <div>
              <p className="text-2xl font-bold">5</p>
              <p className="text-sm text-muted-foreground">Tests Passed</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-amber-500/10 p-3">
              <Trophy className="h-6 w-6 text-amber-500" />
            </div>
            <div>
              <p className="text-2xl font-bold">Top 5%</p>
              <p className="text-sm text-muted-foreground">JavaScript</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-purple-500/10 p-3">
              <TrendingUp className="h-6 w-6 text-purple-500" />
            </div>
            <div>
              <p className="text-2xl font-bold">88%</p>
              <p className="text-sm text-muted-foreground">Avg. Score</p>
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="tests" className="w-full">
        <TabsList>
          <TabsTrigger value="tests">Available Tests</TabsTrigger>
          <TabsTrigger value="certificates">My Certificates</TabsTrigger>
          <TabsTrigger value="progress">Skill Progress</TabsTrigger>
        </TabsList>

        {/* Available Tests Tab */}
        <TabsContent value="tests" className="mt-6 space-y-6">
          {/* Category filter */}
          <div className="flex flex-wrap gap-2">
            {skillCategories.map((category) => (
              <Button
                key={category.id}
                variant={selectedCategory === category.id ? "default" : "outline"}
                size="sm"
                onClick={() => setSelectedCategory(category.id)}
              >
                {category.name}
              </Button>
            ))}
          </div>

          {/* Tests grid */}
          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className="grid gap-4 md:grid-cols-2 lg:grid-cols-3"
          >
            {filteredTests.map((test) => (
              <motion.div key={test.id} variants={itemVariants}>
                <Card className="h-full transition-all hover:shadow-lg">
                  <CardHeader>
                    <div className="flex items-start justify-between">
                      <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-muted text-2xl">
                        {test.icon.startsWith("http") ? (
                          <Image
                            src={test.icon}
                            alt={test.name}
                            width={32}
                            height={32}
                          />
                        ) : (
                          test.icon
                        )}
                      </div>
                      <Badge className={difficultyColors[test.difficulty]}>
                        {test.difficulty}
                      </Badge>
                    </div>
                    <CardTitle className="mt-4">{test.name}</CardTitle>
                    <CardDescription className="line-clamp-2">
                      {test.description}
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <FileText className="h-4 w-4" />
                        {test.questions} questions
                      </div>
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <Timer className="h-4 w-4" />
                        {test.duration} min
                      </div>
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <Target className="h-4 w-4" />
                        {test.passRate}% pass rate
                      </div>
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <BookOpen className="h-4 w-4" />
                        {test.takers.toLocaleString()} takers
                      </div>
                    </div>

                    <div className="flex items-center gap-2 rounded-lg bg-muted/50 p-3">
                      <Award className="h-5 w-5 text-amber-500" />
                      <span className="text-sm">
                        Earn <span className="font-semibold">{test.badge}</span> badge
                      </span>
                    </div>
                  </CardContent>
                  <CardFooter>
                    <Button className="w-full">
                      <Play className="mr-2 h-4 w-4" />
                      Start Test
                    </Button>
                  </CardFooter>
                </Card>
              </motion.div>
            ))}
          </motion.div>
        </TabsContent>

        {/* Certificates Tab */}
        <TabsContent value="certificates" className="mt-6 space-y-6">
          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className="grid gap-4 md:grid-cols-2 lg:grid-cols-3"
          >
            {myCertificates.map((cert) => (
              <motion.div key={cert.id} variants={itemVariants}>
                <Card className={cn(
                  "relative overflow-hidden transition-all hover:shadow-lg",
                  cert.status === "expiring" && "border-amber-500/50"
                )}>
                  {cert.status === "expiring" && (
                    <div className="absolute right-0 top-0 rounded-bl-lg bg-amber-500 px-2 py-1 text-xs font-medium text-white">
                      Expiring Soon
                    </div>
                  )}
                  <CardHeader>
                    <div className="flex items-center gap-4">
                      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-gradient-to-br from-primary/20 to-purple-500/20">
                        {cert.icon.startsWith("http") ? (
                          <Image
                            src={cert.icon}
                            alt={cert.name}
                            width={40}
                            height={40}
                          />
                        ) : (
                          <Award className="h-8 w-8 text-primary" />
                        )}
                      </div>
                      <div>
                        <CardTitle className="text-lg">{cert.name}</CardTitle>
                        <CardDescription>
                          Credential ID: {cert.credentialId}
                        </CardDescription>
                      </div>
                    </div>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-muted-foreground">Score</span>
                      <div className="flex items-center gap-2">
                        <Progress value={cert.score} className="h-2 w-24" />
                        <span className="font-semibold">{cert.score}%</span>
                      </div>
                    </div>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <p className="text-muted-foreground">Issued</p>
                        <p className="font-medium">{cert.issuedDate}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Expires</p>
                        <p className={cn(
                          "font-medium",
                          cert.status === "expiring" && "text-amber-500"
                        )}>
                          {cert.expiryDate}
                        </p>
                      </div>
                    </div>
                  </CardContent>
                  <CardFooter className="flex gap-2">
                    <Button variant="outline" className="flex-1">
                      View
                    </Button>
                    <Button variant="outline" className="flex-1">
                      Share
                    </Button>
                    {cert.status === "expiring" && (
                      <Button className="flex-1">Renew</Button>
                    )}
                  </CardFooter>
                </Card>
              </motion.div>
            ))}
          </motion.div>
        </TabsContent>

        {/* Skill Progress Tab */}
        <TabsContent value="progress" className="mt-6 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Your Skill Levels</CardTitle>
              <CardDescription>
                Based on your test scores and verified certificates
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {skillProgress.map((skill) => (
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
                    <div className="flex items-center gap-2">
                      <span className="text-sm text-muted-foreground">
                        {skill.level < 50
                          ? "Beginner"
                          : skill.level < 70
                          ? "Intermediate"
                          : skill.level < 90
                          ? "Advanced"
                          : "Expert"}
                      </span>
                      <span className="font-semibold">{skill.level}%</span>
                    </div>
                  </div>
                  <Progress value={skill.level} className="h-3" />
                </div>
              ))}
            </CardContent>
            <CardFooter>
              <Button variant="outline" className="w-full">
                <Zap className="mr-2 h-4 w-4" />
                Take More Tests to Improve
              </Button>
            </CardFooter>
          </Card>

          {/* Recommendations */}
          <Card>
            <CardHeader>
              <CardTitle>Recommended Tests</CardTitle>
              <CardDescription>
                Based on your profile and interests
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {availableTests.slice(0, 3).map((test) => (
                  <div
                    key={test.id}
                    className="flex items-center justify-between rounded-lg border p-4"
                  >
                    <div className="flex items-center gap-4">
                      <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-muted">
                        {test.icon.startsWith("http") ? (
                          <Image
                            src={test.icon}
                            alt={test.name}
                            width={24}
                            height={24}
                          />
                        ) : (
                          test.icon
                        )}
                      </div>
                      <div>
                        <p className="font-medium">{test.name}</p>
                        <p className="text-sm text-muted-foreground">
                          {test.questions} questions • {test.duration} min
                        </p>
                      </div>
                    </div>
                    <Button size="sm">
                      Take Test
                      <ChevronRight className="ml-1 h-4 w-4" />
                    </Button>
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
