"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import Link from "next/link";
import {
  Search,
  Building2,
  Clock,
  DollarSign,
  Users,
  Calendar,
  FileText,
  MapPin,
  Eye,
  Download,
  ChevronRight,
  Filter,
  AlertCircle,
  CheckCircle2,
  ArrowUpRight,
  Shield,
  Landmark,
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
import { Progress } from "@/components/ui/progress";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

const tenderCategories = [
  "All",
  "Construction",
  "IT Services",
  "Healthcare",
  "Education",
  "Infrastructure",
  "Supplies",
];

const tenders = [
  {
    id: "tn1",
    title: "Construction of Primary Health Care Center",
    description:
      "Seeking qualified contractors for the construction of a new primary health care center in District Rawalpindi. Project includes building construction, electrical work, plumbing, and medical equipment installation.",
    department: {
      name: "Ministry of Health",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=MH",
      location: "Islamabad, Pakistan",
    },
    budget: { min: 50000000, max: 75000000 },
    currency: "PKR",
    deadline: "Jan 15, 2026",
    submissionDeadline: "Dec 30, 2025",
    bidsReceived: 8,
    category: "Healthcare",
    status: "open",
    referenceNo: "MH-2025-PHC-0234",
    publishedDate: "Nov 25, 2025",
    eligibility: ["Registered Contractor", "Class A License", "5+ Years Experience"],
    documents: ["Technical Specifications", "Bill of Quantities", "Site Plan"],
  },
  {
    id: "tn2",
    title: "IT Infrastructure Modernization Project",
    description:
      "Tender for upgrading IT infrastructure across government offices including servers, networking equipment, cybersecurity systems, and software implementation.",
    department: {
      name: "National IT Board",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=NI",
      location: "Islamabad, Pakistan",
    },
    budget: { min: 100000000, max: 150000000 },
    currency: "PKR",
    deadline: "Feb 1, 2026",
    submissionDeadline: "Jan 20, 2026",
    bidsReceived: 15,
    category: "IT Services",
    status: "open",
    referenceNo: "NITB-2025-INF-0456",
    publishedDate: "Nov 20, 2025",
    eligibility: ["ISO Certified", "Government Clearance", "Technical Team"],
    documents: ["Technical Requirements", "Security Compliance", "Timeline"],
  },
  {
    id: "tn3",
    title: "Road Construction - Lahore Ring Road Extension",
    description:
      "Major infrastructure project for extending the Lahore Ring Road by 25km. Includes road construction, bridges, interchanges, and associated facilities.",
    department: {
      name: "Punjab Infrastructure Dev",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=PI",
      location: "Lahore, Pakistan",
    },
    budget: { min: 500000000, max: 750000000 },
    currency: "PKR",
    deadline: "Mar 15, 2026",
    submissionDeadline: "Feb 28, 2026",
    bidsReceived: 5,
    category: "Infrastructure",
    status: "open",
    referenceNo: "PIDA-2025-RD-0789",
    publishedDate: "Nov 15, 2025",
    eligibility: ["Class A+ License", "JV Allowed", "Bank Guarantee"],
    documents: ["Detailed Design", "Environmental Assessment", "Land Survey"],
  },
  {
    id: "tn4",
    title: "Supply of Medical Equipment for District Hospitals",
    description:
      "Procurement of medical equipment including diagnostic machines, surgical equipment, and patient monitoring systems for 10 district hospitals.",
    department: {
      name: "Health Department Punjab",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=HP",
      location: "Lahore, Pakistan",
    },
    budget: { min: 200000000, max: 300000000 },
    currency: "PKR",
    deadline: "Jan 31, 2026",
    submissionDeadline: "Jan 15, 2026",
    bidsReceived: 22,
    category: "Healthcare",
    status: "evaluation",
    referenceNo: "HDP-2025-MED-0123",
    publishedDate: "Oct 30, 2025",
    eligibility: ["Authorized Distributor", "ISO Certified", "Service Network"],
    documents: ["Equipment List", "Specifications", "Warranty Terms"],
  },
  {
    id: "tn5",
    title: "School Building Construction - Sindh Education",
    description:
      "Construction of 20 new primary school buildings in rural areas of Sindh province. Includes classrooms, libraries, computer labs, and playgrounds.",
    department: {
      name: "Sindh Education Dept",
      avatar: "https://api.dicebear.com/7.x/initials/svg?seed=SE",
      location: "Karachi, Pakistan",
    },
    budget: { min: 150000000, max: 200000000 },
    currency: "PKR",
    deadline: "Feb 15, 2026",
    submissionDeadline: "Jan 31, 2026",
    bidsReceived: 12,
    category: "Education",
    status: "open",
    referenceNo: "SED-2025-SCH-0567",
    publishedDate: "Nov 10, 2025",
    eligibility: ["Construction License", "Previous School Projects", "Local Presence"],
    documents: ["Standard Design", "Site Locations", "Material Specs"],
  },
];

const myBids = [
  {
    id: "b1",
    tenderTitle: "IT Infrastructure Modernization Project",
    department: "National IT Board",
    bidAmount: 125000000,
    submittedDate: "Nov 28, 2025",
    status: "under-review",
    rank: 3,
    totalBids: 15,
  },
  {
    id: "b2",
    tenderTitle: "Supply of Office Equipment",
    department: "Finance Ministry",
    bidAmount: 8500000,
    submittedDate: "Nov 15, 2025",
    status: "shortlisted",
    rank: 2,
    totalBids: 8,
  },
  {
    id: "b3",
    tenderTitle: "Software Development Services",
    department: "NADRA",
    bidAmount: 45000000,
    submittedDate: "Oct 20, 2025",
    status: "awarded",
    rank: 1,
    totalBids: 12,
  },
];

const statusColors: Record<string, string> = {
  open: "bg-green-500/10 text-green-500 border-green-500/20",
  evaluation: "bg-amber-500/10 text-amber-500 border-amber-500/20",
  closed: "bg-gray-500/10 text-gray-500 border-gray-500/20",
  awarded: "bg-blue-500/10 text-blue-500 border-blue-500/20",
  "under-review": "bg-amber-500/10 text-amber-500 border-amber-500/20",
  shortlisted: "bg-purple-500/10 text-purple-500 border-purple-500/20",
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

function formatCurrency(amount: number, currency: string = "PKR") {
  if (currency === "PKR") {
    if (amount >= 10000000) {
      return `${(amount / 10000000).toFixed(1)} Cr`;
    } else if (amount >= 100000) {
      return `${(amount / 100000).toFixed(1)} Lac`;
    }
  }
  return amount.toLocaleString();
}

export default function TendersPage() {
  const [selectedCategory, setSelectedCategory] = useState("All");
  const [searchQuery, setSearchQuery] = useState("");
  const [sortBy, setSortBy] = useState("deadline");

  const filteredTenders = tenders.filter((tender) => {
    if (selectedCategory !== "All" && tender.category !== selectedCategory) {
      return false;
    }
    if (
      searchQuery &&
      !tender.title.toLowerCase().includes(searchQuery.toLowerCase())
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
          <h1 className="text-3xl font-bold">Government Tenders</h1>
          <p className="text-muted-foreground">
            Browse and bid on government procurement opportunities
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="secondary" className="gap-1">
            <AlertCircle className="h-3 w-3" />
            3 Deadlines This Week
          </Badge>
        </div>
      </div>

      {/* Stats overview */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-green-500/10 p-3">
              <FileText className="h-6 w-6 text-green-500" />
            </div>
            <div>
              <p className="text-2xl font-bold">{tenders.filter(t => t.status === "open").length}</p>
              <p className="text-sm text-muted-foreground">Open Tenders</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-blue-500/10 p-3">
              <DollarSign className="h-6 w-6 text-blue-500" />
            </div>
            <div>
              <p className="text-2xl font-bold">1.5B+</p>
              <p className="text-sm text-muted-foreground">Total Value (PKR)</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-purple-500/10 p-3">
              <Users className="h-6 w-6 text-purple-500" />
            </div>
            <div>
              <p className="text-2xl font-bold">3</p>
              <p className="text-sm text-muted-foreground">My Active Bids</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-full bg-amber-500/10 p-3">
              <Trophy className="h-6 w-6 text-amber-500" />
            </div>
            <div>
              <p className="text-2xl font-bold">1</p>
              <p className="text-sm text-muted-foreground">Won This Month</p>
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="browse" className="w-full">
        <TabsList>
          <TabsTrigger value="browse">Browse Tenders</TabsTrigger>
          <TabsTrigger value="my-bids">My Bids</TabsTrigger>
          <TabsTrigger value="awarded">Awarded Contracts</TabsTrigger>
        </TabsList>

        {/* Browse Tenders Tab */}
        <TabsContent value="browse" className="mt-6 space-y-6">
          {/* Filters */}
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div className="relative flex-1 lg:max-w-md">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search tenders..."
                className="pl-9"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <Select value={selectedCategory} onValueChange={setSelectedCategory}>
                <SelectTrigger className="w-[180px]">
                  <SelectValue placeholder="Category" />
                </SelectTrigger>
                <SelectContent>
                  {tenderCategories.map((category) => (
                    <SelectItem key={category} value={category}>
                      {category}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Select value={sortBy} onValueChange={setSortBy}>
                <SelectTrigger className="w-[180px]">
                  <SelectValue placeholder="Sort by" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="deadline">Deadline (Nearest)</SelectItem>
                  <SelectItem value="budget">Budget (Highest)</SelectItem>
                  <SelectItem value="recent">Recently Published</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Tenders list */}
          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className="space-y-4"
          >
            {filteredTenders.map((tender) => (
              <motion.div key={tender.id} variants={itemVariants}>
                <Card className="transition-all hover:shadow-lg">
                  <CardContent className="p-6">
                    <div className="flex flex-col gap-4 lg:flex-row lg:items-start">
                      {/* Main content */}
                      <div className="flex-1 space-y-3">
                        <div className="flex items-start justify-between">
                          <div>
                            <div className="flex items-center gap-2">
                              <Badge
                                variant="outline"
                                className={statusColors[tender.status]}
                              >
                                {tender.status === "open"
                                  ? "Open for Bidding"
                                  : tender.status === "evaluation"
                                  ? "Under Evaluation"
                                  : tender.status}
                              </Badge>
                              <Badge variant="secondary">{tender.category}</Badge>
                            </div>
                            <Link
                              href={`/dashboard/tenders/${tender.id}`}
                              className="mt-2 block text-xl font-semibold hover:text-primary"
                            >
                              {tender.title}
                            </Link>
                            <p className="text-sm text-muted-foreground">
                              Ref: {tender.referenceNo}
                            </p>
                          </div>
                        </div>

                        <p className="line-clamp-2 text-muted-foreground">
                          {tender.description}
                        </p>

                        {/* Department info */}
                        <div className="flex items-center gap-3">
                          <Avatar className="h-10 w-10">
                            <AvatarImage src={tender.department.avatar} />
                            <AvatarFallback>
                              {tender.department.name[0]}
                            </AvatarFallback>
                          </Avatar>
                          <div>
                            <p className="font-medium">{tender.department.name}</p>
                            <p className="flex items-center gap-1 text-sm text-muted-foreground">
                              <MapPin className="h-3 w-3" />
                              {tender.department.location}
                            </p>
                          </div>
                        </div>

                        {/* Key info */}
                        <div className="flex flex-wrap items-center gap-4 text-sm">
                          <div className="flex items-center gap-1">
                            <DollarSign className="h-4 w-4 text-green-500" />
                            <span className="font-semibold">
                              {formatCurrency(tender.budget.min)} -{" "}
                              {formatCurrency(tender.budget.max)} {tender.currency}
                            </span>
                          </div>
                          <div className="flex items-center gap-1 text-muted-foreground">
                            <Users className="h-4 w-4" />
                            {tender.bidsReceived} bids received
                          </div>
                          <div className="flex items-center gap-1 text-muted-foreground">
                            <Calendar className="h-4 w-4" />
                            Published: {tender.publishedDate}
                          </div>
                        </div>

                        {/* Eligibility tags */}
                        <div className="flex flex-wrap gap-1">
                          {tender.eligibility.map((item) => (
                            <Badge key={item} variant="outline" className="text-xs">
                              <Shield className="mr-1 h-3 w-3" />
                              {item}
                            </Badge>
                          ))}
                        </div>
                      </div>

                      {/* Sidebar info */}
                      <div className="flex flex-col gap-3 rounded-lg border p-4 lg:w-64">
                        <div className="text-center">
                          <p className="text-sm text-muted-foreground">
                            Submission Deadline
                          </p>
                          <p className="text-lg font-bold text-primary">
                            {tender.submissionDeadline}
                          </p>
                        </div>

                        <div className="space-y-2 text-sm">
                          <div className="flex items-center justify-between">
                            <span className="text-muted-foreground">
                              Project Deadline
                            </span>
                            <span>{tender.deadline}</span>
                          </div>
                          <div className="flex items-center justify-between">
                            <span className="text-muted-foreground">Documents</span>
                            <span>{tender.documents.length} files</span>
                          </div>
                        </div>

                        <div className="space-y-2">
                          <Button className="w-full">
                            Submit Bid
                            <ArrowUpRight className="ml-2 h-4 w-4" />
                          </Button>
                          <Button variant="outline" className="w-full">
                            <Download className="mr-2 h-4 w-4" />
                            Download Documents
                          </Button>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </motion.div>
            ))}
          </motion.div>
        </TabsContent>

        {/* My Bids Tab */}
        <TabsContent value="my-bids" className="mt-6 space-y-6">
          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className="space-y-4"
          >
            {myBids.map((bid) => (
              <motion.div key={bid.id} variants={itemVariants}>
                <Card>
                  <CardContent className="p-6">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-4">
                        <div className="rounded-full bg-primary/10 p-3">
                          <Landmark className="h-6 w-6 text-primary" />
                        </div>
                        <div>
                          <h3 className="font-semibold">{bid.tenderTitle}</h3>
                          <p className="text-sm text-muted-foreground">
                            {bid.department} • Submitted: {bid.submittedDate}
                          </p>
                        </div>
                      </div>
                      <div className="flex items-center gap-4">
                        <div className="text-right">
                          <p className="text-lg font-bold">
                            {formatCurrency(bid.bidAmount)} PKR
                          </p>
                          <p className="text-sm text-muted-foreground">
                            Rank #{bid.rank} of {bid.totalBids}
                          </p>
                        </div>
                        <Badge
                          variant="outline"
                          className={statusColors[bid.status]}
                        >
                          {bid.status === "under-review"
                            ? "Under Review"
                            : bid.status === "shortlisted"
                            ? "Shortlisted"
                            : bid.status === "awarded"
                            ? "Awarded"
                            : bid.status}
                        </Badge>
                      </div>
                    </div>
                    {bid.status === "awarded" && (
                      <div className="mt-4 flex items-center gap-2 rounded-lg bg-green-500/10 p-3 text-green-600">
                        <CheckCircle2 className="h-5 w-5" />
                        <span className="font-medium">
                          Congratulations! Your bid has been awarded.
                        </span>
                        <Button size="sm" className="ml-auto">
                          View Contract
                        </Button>
                      </div>
                    )}
                  </CardContent>
                </Card>
              </motion.div>
            ))}
          </motion.div>
        </TabsContent>

        {/* Awarded Contracts Tab */}
        <TabsContent value="awarded" className="mt-6">
          <Card>
            <CardHeader>
              <CardTitle>Awarded Contracts</CardTitle>
              <CardDescription>
                Your successfully won government contracts
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="flex items-center gap-4">
                    <div className="rounded-full bg-green-500/10 p-2">
                      <CheckCircle2 className="h-5 w-5 text-green-500" />
                    </div>
                    <div>
                      <p className="font-semibold">Software Development Services</p>
                      <p className="text-sm text-muted-foreground">
                        NADRA • Contract Value: 4.5 Cr PKR
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="text-right">
                      <p className="text-sm text-muted-foreground">Progress</p>
                      <div className="flex items-center gap-2">
                        <Progress value={35} className="h-2 w-24" />
                        <span className="font-medium">35%</span>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      Manage
                      <ChevronRight className="ml-1 h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}

function Trophy(props: React.SVGProps<SVGSVGElement>) {
  return (
    <svg
      {...props}
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M6 9H4.5a2.5 2.5 0 0 1 0-5H6" />
      <path d="M18 9h1.5a2.5 2.5 0 0 0 0-5H18" />
      <path d="M4 22h16" />
      <path d="M10 14.66V17c0 .55-.47.98-.97 1.21C7.85 18.75 7 20.24 7 22" />
      <path d="M14 14.66V17c0 .55.47.98.97 1.21C16.15 18.75 17 20.24 17 22" />
      <path d="M18 2H6v7a6 6 0 0 0 12 0V2Z" />
    </svg>
  );
}
