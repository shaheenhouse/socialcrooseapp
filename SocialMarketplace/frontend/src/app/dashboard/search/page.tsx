"use client";

import { useState, useEffect } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { motion, AnimatePresence } from "framer-motion";
import {
  Search,
  Filter,
  User,
  Briefcase,
  Building2,
  FileText,
  ShoppingBag,
  MapPin,
  Star,
  Clock,
  DollarSign,
  X,
  SlidersHorizontal,
} from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Slider } from "@/components/ui/slider";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { cn } from "@/lib/utils";

// Mock search results
const mockResults = {
  people: [
    {
      id: "1",
      name: "Sarah Johnson",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=sarah",
      headline: "Senior Full Stack Developer at TechCorp",
      location: "San Francisco, CA",
      connections: 248,
      mutualConnections: 12,
      skills: ["React", "Node.js", "TypeScript"],
    },
    {
      id: "2",
      name: "Ahmed Khan",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=ahmed",
      headline: "Product Manager | Building the Future",
      location: "New York, NY",
      connections: 512,
      mutualConnections: 8,
      skills: ["Product Strategy", "Agile", "Data Analysis"],
    },
    {
      id: "3",
      name: "Maria Garcia",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=maria",
      headline: "UX Designer & Creative Director",
      location: "Los Angeles, CA",
      connections: 324,
      mutualConnections: 5,
      skills: ["Figma", "UI Design", "User Research"],
    },
  ],
  services: [
    {
      id: "1",
      title: "Professional Logo Design",
      description: "I will create a stunning, modern logo for your brand",
      seller: "DesignStudio",
      sellerAvatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=design",
      price: 50,
      rating: 4.9,
      reviews: 234,
      deliveryDays: 3,
      image: "https://picsum.photos/seed/logo/300/200",
    },
    {
      id: "2",
      title: "Full Website Development",
      description: "Complete responsive website with modern technologies",
      seller: "WebExperts",
      sellerAvatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=web",
      price: 500,
      rating: 4.8,
      reviews: 156,
      deliveryDays: 14,
      image: "https://picsum.photos/seed/web/300/200",
    },
  ],
  projects: [
    {
      id: "1",
      title: "E-commerce Platform Development",
      description: "Looking for an experienced developer to build a complete e-commerce solution",
      budget: { min: 5000, max: 10000 },
      bids: 23,
      postedAt: "2024-02-25",
      deadline: "2024-03-25",
      skills: ["React", "Node.js", "PostgreSQL"],
    },
    {
      id: "2",
      title: "Mobile App UI/UX Redesign",
      description: "Need a designer to completely redesign our mobile app interface",
      budget: { min: 2000, max: 4000 },
      bids: 15,
      postedAt: "2024-02-24",
      deadline: "2024-03-15",
      skills: ["Figma", "Mobile Design", "UI/UX"],
    },
  ],
  companies: [
    {
      id: "1",
      name: "TechCorp Inc.",
      logo: "https://api.dicebear.com/7.x/shapes/svg?seed=techcorp",
      industry: "Technology",
      description: "Leading technology solutions provider",
      location: "San Francisco, CA",
      size: "1000-5000 employees",
      followers: 15000,
    },
    {
      id: "2",
      name: "DesignHub Agency",
      logo: "https://api.dicebear.com/7.x/shapes/svg?seed=designhub",
      industry: "Design",
      description: "Creative design agency for digital products",
      location: "New York, NY",
      size: "50-200 employees",
      followers: 8500,
    },
  ],
};

const searchTypes = [
  { value: "all", label: "All", icon: Search },
  { value: "people", label: "People", icon: User },
  { value: "services", label: "Services", icon: ShoppingBag },
  { value: "projects", label: "Projects", icon: Briefcase },
  { value: "companies", label: "Companies", icon: Building2 },
  { value: "posts", label: "Posts", icon: FileText },
];

export default function SearchPage() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const [query, setQuery] = useState(searchParams.get("q") || "");
  const [activeTab, setActiveTab] = useState(searchParams.get("type") || "all");
  const [priceRange, setPriceRange] = useState([0, 10000]);
  const [showFilters, setShowFilters] = useState(false);

  const handleSearch = (newQuery?: string) => {
    const q = newQuery ?? query;
    if (!q.trim()) return;
    const params = new URLSearchParams({ q, type: activeTab });
    router.push(`/dashboard/search?${params.toString()}`);
  };

  const handleTabChange = (value: string) => {
    setActiveTab(value);
    if (query) {
      const params = new URLSearchParams({ q: query, type: value });
      router.push(`/dashboard/search?${params.toString()}`);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-primary/5 p-6">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Search Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="space-y-4"
        >
          <div className="flex items-center gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground" />
              <Input
                placeholder="Search people, services, projects, companies..."
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && handleSearch()}
                className="pl-12 h-12 text-lg"
              />
            </div>
            <Button onClick={() => handleSearch()} size="lg" className="h-12 px-8">
              Search
            </Button>
            <Sheet open={showFilters} onOpenChange={setShowFilters}>
              <SheetTrigger asChild>
                <Button variant="outline" size="lg" className="h-12 gap-2">
                  <SlidersHorizontal className="h-4 w-4" />
                  Filters
                </Button>
              </SheetTrigger>
              <SheetContent>
                <SheetHeader>
                  <SheetTitle>Search Filters</SheetTitle>
                  <SheetDescription>
                    Refine your search results
                  </SheetDescription>
                </SheetHeader>
                <div className="mt-6 space-y-6">
                  {/* Location Filter */}
                  <div>
                    <label className="text-sm font-medium mb-2 block">Location</label>
                    <Input placeholder="Enter location..." />
                  </div>

                  {/* Price Range */}
                  <div>
                    <label className="text-sm font-medium mb-2 block">
                      Price Range: ${priceRange[0]} - ${priceRange[1]}
                    </label>
                    <Slider
                      value={priceRange}
                      onValueChange={setPriceRange}
                      min={0}
                      max={10000}
                      step={100}
                      className="mt-4"
                    />
                  </div>

                  <Separator />

                  {/* Skills Filter */}
                  <div>
                    <label className="text-sm font-medium mb-3 block">Skills</label>
                    <div className="space-y-2">
                      {["React", "Node.js", "Python", "TypeScript", "Figma"].map((skill) => (
                        <div key={skill} className="flex items-center space-x-2">
                          <Checkbox id={skill} />
                          <label htmlFor={skill} className="text-sm">
                            {skill}
                          </label>
                        </div>
                      ))}
                    </div>
                  </div>

                  <Separator />

                  {/* Sort By */}
                  <div>
                    <label className="text-sm font-medium mb-2 block">Sort By</label>
                    <Select defaultValue="relevance">
                      <SelectTrigger>
                        <SelectValue placeholder="Sort by" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="relevance">Relevance</SelectItem>
                        <SelectItem value="recent">Most Recent</SelectItem>
                        <SelectItem value="rating">Highest Rated</SelectItem>
                        <SelectItem value="price-low">Price: Low to High</SelectItem>
                        <SelectItem value="price-high">Price: High to Low</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="flex gap-2 mt-8">
                    <Button variant="outline" className="flex-1">
                      Clear All
                    </Button>
                    <Button className="flex-1" onClick={() => setShowFilters(false)}>
                      Apply Filters
                    </Button>
                  </div>
                </div>
              </SheetContent>
            </Sheet>
          </div>

          {/* Search Type Tabs */}
          <Tabs value={activeTab} onValueChange={handleTabChange}>
            <TabsList className="h-auto flex-wrap justify-start">
              {searchTypes.map((type) => (
                <TabsTrigger
                  key={type.value}
                  value={type.value}
                  className="gap-2 data-[state=active]:bg-primary data-[state=active]:text-primary-foreground"
                >
                  <type.icon className="h-4 w-4" />
                  {type.label}
                </TabsTrigger>
              ))}
            </TabsList>
          </Tabs>
        </motion.div>

        {/* Search Results */}
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          {/* Main Results */}
          <div className="lg:col-span-3 space-y-4">
            {/* Results Summary */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className="flex items-center justify-between"
            >
              <p className="text-muted-foreground">
                Showing results for &ldquo;<span className="font-medium text-foreground">{query || "all"}</span>&rdquo;
              </p>
              <Select defaultValue="relevance">
                <SelectTrigger className="w-40">
                  <SelectValue placeholder="Sort by" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="relevance">Relevance</SelectItem>
                  <SelectItem value="recent">Most Recent</SelectItem>
                </SelectContent>
              </Select>
            </motion.div>

            {/* People Results */}
            {(activeTab === "all" || activeTab === "people") && (
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                className="space-y-3"
              >
                {activeTab === "all" && (
                  <div className="flex items-center justify-between">
                    <h2 className="text-lg font-semibold flex items-center gap-2">
                      <User className="h-5 w-5" />
                      People
                    </h2>
                    <Button variant="ghost" size="sm" onClick={() => handleTabChange("people")}>
                      See all
                    </Button>
                  </div>
                )}
                {mockResults.people.map((person) => (
                  <Card key={person.id} className="hover:shadow-lg transition-shadow cursor-pointer">
                    <CardContent className="p-4">
                      <div className="flex items-center gap-4">
                        <Avatar className="h-16 w-16 ring-2 ring-primary/20">
                          <AvatarImage src={person.avatar} />
                          <AvatarFallback>{person.name[0]}</AvatarFallback>
                        </Avatar>
                        <div className="flex-1 min-w-0">
                          <h3 className="font-semibold text-lg">{person.name}</h3>
                          <p className="text-muted-foreground">{person.headline}</p>
                          <div className="flex items-center gap-4 mt-2 text-sm text-muted-foreground">
                            <span className="flex items-center gap-1">
                              <MapPin className="h-4 w-4" />
                              {person.location}
                            </span>
                            <span>{person.mutualConnections} mutual connections</span>
                          </div>
                          <div className="flex gap-2 mt-2">
                            {person.skills.map((skill) => (
                              <Badge key={skill} variant="secondary">
                                {skill}
                              </Badge>
                            ))}
                          </div>
                        </div>
                        <Button>Connect</Button>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </motion.div>
            )}

            {/* Services Results */}
            {(activeTab === "all" || activeTab === "services") && (
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.1 }}
                className="space-y-3"
              >
                {activeTab === "all" && (
                  <div className="flex items-center justify-between mt-6">
                    <h2 className="text-lg font-semibold flex items-center gap-2">
                      <ShoppingBag className="h-5 w-5" />
                      Services
                    </h2>
                    <Button variant="ghost" size="sm" onClick={() => handleTabChange("services")}>
                      See all
                    </Button>
                  </div>
                )}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {mockResults.services.map((service) => (
                    <Card key={service.id} className="hover:shadow-lg transition-shadow cursor-pointer overflow-hidden">
                      <img
                        src={service.image}
                        alt={service.title}
                        className="w-full h-40 object-cover"
                      />
                      <CardContent className="p-4">
                        <div className="flex items-center gap-2 mb-2">
                          <Avatar className="h-6 w-6">
                            <AvatarImage src={service.sellerAvatar} />
                            <AvatarFallback>{service.seller[0]}</AvatarFallback>
                          </Avatar>
                          <span className="text-sm text-muted-foreground">{service.seller}</span>
                        </div>
                        <h3 className="font-semibold">{service.title}</h3>
                        <p className="text-sm text-muted-foreground line-clamp-2 mt-1">
                          {service.description}
                        </p>
                        <div className="flex items-center justify-between mt-3">
                          <div className="flex items-center gap-1">
                            <Star className="h-4 w-4 fill-yellow-500 text-yellow-500" />
                            <span className="font-medium">{service.rating}</span>
                            <span className="text-muted-foreground">({service.reviews})</span>
                          </div>
                          <p className="font-bold text-lg">
                            From ${service.price}
                          </p>
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </motion.div>
            )}

            {/* Projects Results */}
            {(activeTab === "all" || activeTab === "projects") && (
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2 }}
                className="space-y-3"
              >
                {activeTab === "all" && (
                  <div className="flex items-center justify-between mt-6">
                    <h2 className="text-lg font-semibold flex items-center gap-2">
                      <Briefcase className="h-5 w-5" />
                      Projects
                    </h2>
                    <Button variant="ghost" size="sm" onClick={() => handleTabChange("projects")}>
                      See all
                    </Button>
                  </div>
                )}
                {mockResults.projects.map((project) => (
                  <Card key={project.id} className="hover:shadow-lg transition-shadow cursor-pointer">
                    <CardContent className="p-4">
                      <h3 className="font-semibold text-lg">{project.title}</h3>
                      <p className="text-muted-foreground mt-1">{project.description}</p>
                      <div className="flex flex-wrap gap-2 mt-3">
                        {project.skills.map((skill) => (
                          <Badge key={skill} variant="outline">
                            {skill}
                          </Badge>
                        ))}
                      </div>
                      <div className="flex items-center justify-between mt-4">
                        <div className="flex items-center gap-4 text-sm text-muted-foreground">
                          <span className="flex items-center gap-1">
                            <DollarSign className="h-4 w-4" />
                            ${project.budget.min.toLocaleString()} - ${project.budget.max.toLocaleString()}
                          </span>
                          <span>{project.bids} bids</span>
                        </div>
                        <Button>View Project</Button>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </motion.div>
            )}

            {/* Companies Results */}
            {(activeTab === "all" || activeTab === "companies") && (
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.3 }}
                className="space-y-3"
              >
                {activeTab === "all" && (
                  <div className="flex items-center justify-between mt-6">
                    <h2 className="text-lg font-semibold flex items-center gap-2">
                      <Building2 className="h-5 w-5" />
                      Companies
                    </h2>
                    <Button variant="ghost" size="sm" onClick={() => handleTabChange("companies")}>
                      See all
                    </Button>
                  </div>
                )}
                {mockResults.companies.map((company) => (
                  <Card key={company.id} className="hover:shadow-lg transition-shadow cursor-pointer">
                    <CardContent className="p-4">
                      <div className="flex items-center gap-4">
                        <Avatar className="h-16 w-16 rounded-lg">
                          <AvatarImage src={company.logo} />
                          <AvatarFallback className="rounded-lg">{company.name[0]}</AvatarFallback>
                        </Avatar>
                        <div className="flex-1 min-w-0">
                          <h3 className="font-semibold text-lg">{company.name}</h3>
                          <p className="text-muted-foreground">{company.description}</p>
                          <div className="flex items-center gap-4 mt-2 text-sm text-muted-foreground">
                            <span>{company.industry}</span>
                            <span>{company.size}</span>
                            <span>{company.followers.toLocaleString()} followers</span>
                          </div>
                        </div>
                        <Button variant="outline">Follow</Button>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </motion.div>
            )}
          </div>

          {/* Sidebar */}
          <div className="hidden lg:block">
            <motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.2 }}
            >
              <Card className="sticky top-6">
                <CardContent className="p-4 space-y-4">
                  <h3 className="font-semibold">Quick Filters</h3>
                  
                  <div>
                    <label className="text-sm font-medium mb-2 block">Location</label>
                    <Input placeholder="Any location" />
                  </div>

                  <div>
                    <label className="text-sm font-medium mb-2 block">Industry</label>
                    <Select>
                      <SelectTrigger>
                        <SelectValue placeholder="All industries" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="technology">Technology</SelectItem>
                        <SelectItem value="design">Design</SelectItem>
                        <SelectItem value="marketing">Marketing</SelectItem>
                        <SelectItem value="finance">Finance</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div>
                    <label className="text-sm font-medium mb-2 block">Experience Level</label>
                    <div className="space-y-2">
                      {["Entry", "Mid", "Senior", "Lead"].map((level) => (
                        <div key={level} className="flex items-center space-x-2">
                          <Checkbox id={level} />
                          <label htmlFor={level} className="text-sm">
                            {level} Level
                          </label>
                        </div>
                      ))}
                    </div>
                  </div>

                  <Button className="w-full">Apply Filters</Button>
                </CardContent>
              </Card>
            </motion.div>
          </div>
        </div>
      </div>
    </div>
  );
}
