"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import Image from "next/image";
import Link from "next/link";
import {
  Search,
  Filter,
  Grid,
  List,
  Star,
  Heart,
  MapPin,
  Clock,
  ChevronDown,
  SlidersHorizontal,
  X,
  Check,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
} from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { Slider } from "@/components/ui/slider";
import { Checkbox } from "@/components/ui/checkbox";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

const categories = [
  { id: "all", name: "All Categories", count: 2450 },
  { id: "web-dev", name: "Web Development", count: 543 },
  { id: "mobile-dev", name: "Mobile Development", count: 312 },
  { id: "design", name: "Design & Graphics", count: 428 },
  { id: "marketing", name: "Digital Marketing", count: 267 },
  { id: "writing", name: "Writing & Content", count: 389 },
  { id: "video", name: "Video & Animation", count: 198 },
  { id: "consulting", name: "Business Consulting", count: 156 },
  { id: "data", name: "Data & Analytics", count: 157 },
];

const services = [
  {
    id: "1",
    title: "Professional Website Development",
    description: "Full-stack web development with modern technologies like React, Next.js, and Node.js",
    seller: {
      name: "John Developer",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=John",
      level: "Top Rated",
      location: "Lahore, Pakistan",
    },
    price: 500,
    rating: 4.9,
    reviews: 234,
    deliveryTime: "7 days",
    category: "web-dev",
    tags: ["React", "Next.js", "Node.js"],
    image: "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=400&h=300&fit=crop",
    isFeatured: true,
    isNew: false,
  },
  {
    id: "2",
    title: "Mobile App UI/UX Design",
    description: "Beautiful and intuitive mobile app designs for iOS and Android platforms",
    seller: {
      name: "Sarah Designer",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Sarah",
      level: "Pro",
      location: "Karachi, Pakistan",
    },
    price: 350,
    rating: 4.8,
    reviews: 189,
    deliveryTime: "5 days",
    category: "design",
    tags: ["Figma", "UI/UX", "Mobile"],
    image: "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=400&h=300&fit=crop",
    isFeatured: true,
    isNew: true,
  },
  {
    id: "3",
    title: "SEO & Content Marketing",
    description: "Comprehensive SEO strategy and content marketing to boost your online presence",
    seller: {
      name: "Mike Marketing",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Mike",
      level: "Expert",
      location: "Islamabad, Pakistan",
    },
    price: 250,
    rating: 4.7,
    reviews: 156,
    deliveryTime: "14 days",
    category: "marketing",
    tags: ["SEO", "Content", "Analytics"],
    image: "https://images.unsplash.com/photo-1432888622747-4eb9a8efeb07?w=400&h=300&fit=crop",
    isFeatured: false,
    isNew: false,
  },
  {
    id: "4",
    title: "Flutter Mobile App Development",
    description: "Cross-platform mobile app development with Flutter and Firebase",
    seller: {
      name: "Ali Flutter",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Ali",
      level: "Top Rated",
      location: "Rawalpindi, Pakistan",
    },
    price: 800,
    rating: 5.0,
    reviews: 98,
    deliveryTime: "14 days",
    category: "mobile-dev",
    tags: ["Flutter", "Dart", "Firebase"],
    image: "https://images.unsplash.com/photo-1551650975-87deedd944c3?w=400&h=300&fit=crop",
    isFeatured: true,
    isNew: false,
  },
  {
    id: "5",
    title: "Brand Identity Design",
    description: "Complete brand identity package including logo, colors, and brand guidelines",
    seller: {
      name: "Emma Brand",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Emma",
      level: "Pro",
      location: "Faisalabad, Pakistan",
    },
    price: 450,
    rating: 4.9,
    reviews: 212,
    deliveryTime: "10 days",
    category: "design",
    tags: ["Branding", "Logo", "Identity"],
    image: "https://images.unsplash.com/photo-1626785774573-4b799315345d?w=400&h=300&fit=crop",
    isFeatured: false,
    isNew: true,
  },
  {
    id: "6",
    title: "Video Editing & Production",
    description: "Professional video editing with motion graphics and color grading",
    seller: {
      name: "James Video",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=James",
      level: "Expert",
      location: "Multan, Pakistan",
    },
    price: 300,
    rating: 4.6,
    reviews: 143,
    deliveryTime: "3 days",
    category: "video",
    tags: ["Video", "Motion", "Editing"],
    image: "https://images.unsplash.com/photo-1574717024453-354056aafa98?w=400&h=300&fit=crop",
    isFeatured: false,
    isNew: false,
  },
  {
    id: "7",
    title: "Data Analytics & Visualization",
    description: "Transform your data into actionable insights with beautiful dashboards",
    seller: {
      name: "Lisa Data",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Lisa",
      level: "Top Rated",
      location: "Peshawar, Pakistan",
    },
    price: 600,
    rating: 4.8,
    reviews: 87,
    deliveryTime: "7 days",
    category: "data",
    tags: ["Python", "Power BI", "Analytics"],
    image: "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400&h=300&fit=crop",
    isFeatured: true,
    isNew: false,
  },
  {
    id: "8",
    title: "Technical Content Writing",
    description: "High-quality technical documentation and blog posts for tech companies",
    seller: {
      name: "Tom Writer",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Tom",
      level: "Pro",
      location: "Lahore, Pakistan",
    },
    price: 150,
    rating: 4.7,
    reviews: 278,
    deliveryTime: "5 days",
    category: "writing",
    tags: ["Technical", "Blog", "Documentation"],
    image: "https://images.unsplash.com/photo-1455390582262-044cdead277a?w=400&h=300&fit=crop",
    isFeatured: false,
    isNew: true,
  },
];

const sortOptions = [
  { value: "relevance", label: "Most Relevant" },
  { value: "rating", label: "Highest Rated" },
  { value: "reviews", label: "Most Reviews" },
  { value: "price-low", label: "Price: Low to High" },
  { value: "price-high", label: "Price: High to Low" },
  { value: "newest", label: "Newest First" },
];

const sellerLevels = ["Top Rated", "Pro", "Expert", "New"];

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.05,
    },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: {
    opacity: 1,
    y: 0,
    transition: {
      duration: 0.3,
    },
  },
};

export default function MarketplacePage() {
  const [viewMode, setViewMode] = useState<"grid" | "list">("grid");
  const [selectedCategory, setSelectedCategory] = useState("all");
  const [sortBy, setSortBy] = useState("relevance");
  const [priceRange, setPriceRange] = useState([0, 1000]);
  const [selectedLevels, setSelectedLevels] = useState<string[]>([]);
  const [searchQuery, setSearchQuery] = useState("");

  const filteredServices = services.filter((service) => {
    if (selectedCategory !== "all" && service.category !== selectedCategory) {
      return false;
    }
    if (service.price < priceRange[0] || service.price > priceRange[1]) {
      return false;
    }
    if (selectedLevels.length > 0 && !selectedLevels.includes(service.seller.level)) {
      return false;
    }
    if (
      searchQuery &&
      !service.title.toLowerCase().includes(searchQuery.toLowerCase()) &&
      !service.description.toLowerCase().includes(searchQuery.toLowerCase())
    ) {
      return false;
    }
    return true;
  });

  const toggleLevel = (level: string) => {
    setSelectedLevels((prev) =>
      prev.includes(level) ? prev.filter((l) => l !== level) : [...prev, level]
    );
  };

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold">Marketplace</h1>
          <p className="text-muted-foreground">
            Discover amazing services from talented professionals
          </p>
        </div>
      </div>

      {/* Search and filters bar */}
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div className="relative flex-1 lg:max-w-md">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search services..."
            className="pl-9"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {/* Mobile filter button */}
          <Sheet>
            <SheetTrigger asChild>
              <Button variant="outline" className="lg:hidden">
                <SlidersHorizontal className="mr-2 h-4 w-4" />
                Filters
              </Button>
            </SheetTrigger>
            <SheetContent side="left" className="w-[300px]">
              <SheetHeader>
                <SheetTitle>Filters</SheetTitle>
                <SheetDescription>
                  Refine your search results
                </SheetDescription>
              </SheetHeader>
              <div className="mt-6 space-y-6">
                {/* Categories */}
                <div>
                  <h4 className="mb-3 font-medium">Categories</h4>
                  <div className="space-y-2">
                    {categories.map((category) => (
                      <button
                        key={category.id}
                        onClick={() => setSelectedCategory(category.id)}
                        className={cn(
                          "flex w-full items-center justify-between rounded-lg px-3 py-2 text-sm transition-colors",
                          selectedCategory === category.id
                            ? "bg-primary text-primary-foreground"
                            : "hover:bg-muted"
                        )}
                      >
                        <span>{category.name}</span>
                        <span className="text-xs opacity-70">{category.count}</span>
                      </button>
                    ))}
                  </div>
                </div>

                {/* Price range */}
                <div>
                  <h4 className="mb-3 font-medium">Price Range</h4>
                  <Slider
                    value={priceRange}
                    onValueChange={setPriceRange}
                    max={1000}
                    step={50}
                    className="mb-2"
                  />
                  <div className="flex items-center justify-between text-sm text-muted-foreground">
                    <span>${priceRange[0]}</span>
                    <span>${priceRange[1]}</span>
                  </div>
                </div>

                {/* Seller level */}
                <div>
                  <h4 className="mb-3 font-medium">Seller Level</h4>
                  <div className="space-y-2">
                    {sellerLevels.map((level) => (
                      <label
                        key={level}
                        className="flex cursor-pointer items-center gap-2"
                      >
                        <Checkbox
                          checked={selectedLevels.includes(level)}
                          onCheckedChange={() => toggleLevel(level)}
                        />
                        <span className="text-sm">{level}</span>
                      </label>
                    ))}
                  </div>
                </div>
              </div>
            </SheetContent>
          </Sheet>

          {/* Sort dropdown */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline">
                Sort by
                <ChevronDown className="ml-2 h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {sortOptions.map((option) => (
                <DropdownMenuItem
                  key={option.value}
                  onClick={() => setSortBy(option.value)}
                  className="flex items-center justify-between"
                >
                  {option.label}
                  {sortBy === option.value && (
                    <Check className="h-4 w-4 text-primary" />
                  )}
                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>

          {/* View mode toggle */}
          <div className="flex rounded-lg border">
            <Button
              variant={viewMode === "grid" ? "secondary" : "ghost"}
              size="icon"
              onClick={() => setViewMode("grid")}
            >
              <Grid className="h-4 w-4" />
            </Button>
            <Button
              variant={viewMode === "list" ? "secondary" : "ghost"}
              size="icon"
              onClick={() => setViewMode("list")}
            >
              <List className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </div>

      {/* Main content with sidebar */}
      <div className="flex gap-6">
        {/* Desktop sidebar */}
        <aside className="hidden w-64 shrink-0 lg:block">
          <div className="sticky top-24 space-y-6">
            {/* Categories */}
            <Card>
              <CardHeader className="pb-3">
                <h4 className="font-semibold">Categories</h4>
              </CardHeader>
              <CardContent className="pt-0">
                <div className="space-y-1">
                  {categories.map((category) => (
                    <button
                      key={category.id}
                      onClick={() => setSelectedCategory(category.id)}
                      className={cn(
                        "flex w-full items-center justify-between rounded-lg px-3 py-2 text-sm transition-colors",
                        selectedCategory === category.id
                          ? "bg-primary text-primary-foreground"
                          : "hover:bg-muted"
                      )}
                    >
                      <span>{category.name}</span>
                      <span className="text-xs opacity-70">{category.count}</span>
                    </button>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Price range */}
            <Card>
              <CardHeader className="pb-3">
                <h4 className="font-semibold">Price Range</h4>
              </CardHeader>
              <CardContent className="pt-0">
                <Slider
                  value={priceRange}
                  onValueChange={setPriceRange}
                  max={1000}
                  step={50}
                  className="mb-2"
                />
                <div className="flex items-center justify-between text-sm text-muted-foreground">
                  <span>${priceRange[0]}</span>
                  <span>${priceRange[1]}</span>
                </div>
              </CardContent>
            </Card>

            {/* Seller level */}
            <Card>
              <CardHeader className="pb-3">
                <h4 className="font-semibold">Seller Level</h4>
              </CardHeader>
              <CardContent className="pt-0">
                <div className="space-y-2">
                  {sellerLevels.map((level) => (
                    <label
                      key={level}
                      className="flex cursor-pointer items-center gap-2"
                    >
                      <Checkbox
                        checked={selectedLevels.includes(level)}
                        onCheckedChange={() => toggleLevel(level)}
                      />
                      <span className="text-sm">{level}</span>
                    </label>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </aside>

        {/* Services grid/list */}
        <div className="flex-1">
          {/* Active filters */}
          {(selectedCategory !== "all" ||
            selectedLevels.length > 0 ||
            priceRange[0] > 0 ||
            priceRange[1] < 1000) && (
            <div className="mb-4 flex flex-wrap items-center gap-2">
              <span className="text-sm text-muted-foreground">Active filters:</span>
              {selectedCategory !== "all" && (
                <Badge variant="secondary" className="gap-1">
                  {categories.find((c) => c.id === selectedCategory)?.name}
                  <X
                    className="h-3 w-3 cursor-pointer"
                    onClick={() => setSelectedCategory("all")}
                  />
                </Badge>
              )}
              {selectedLevels.map((level) => (
                <Badge key={level} variant="secondary" className="gap-1">
                  {level}
                  <X
                    className="h-3 w-3 cursor-pointer"
                    onClick={() => toggleLevel(level)}
                  />
                </Badge>
              ))}
              {(priceRange[0] > 0 || priceRange[1] < 1000) && (
                <Badge variant="secondary" className="gap-1">
                  ${priceRange[0]} - ${priceRange[1]}
                  <X
                    className="h-3 w-3 cursor-pointer"
                    onClick={() => setPriceRange([0, 1000])}
                  />
                </Badge>
              )}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  setSelectedCategory("all");
                  setSelectedLevels([]);
                  setPriceRange([0, 1000]);
                }}
              >
                Clear all
              </Button>
            </div>
          )}

          {/* Results count */}
          <p className="mb-4 text-sm text-muted-foreground">
            Showing {filteredServices.length} services
          </p>

          {/* Services */}
          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className={cn(
              viewMode === "grid"
                ? "grid gap-4 sm:grid-cols-2 xl:grid-cols-3"
                : "space-y-4"
            )}
          >
            {filteredServices.map((service) => (
              <motion.div key={service.id} variants={itemVariants}>
                <ServiceCard service={service} viewMode={viewMode} />
              </motion.div>
            ))}
          </motion.div>

          {filteredServices.length === 0 && (
            <div className="flex flex-col items-center justify-center py-12">
              <Search className="h-12 w-12 text-muted-foreground/50" />
              <h3 className="mt-4 text-lg font-semibold">No services found</h3>
              <p className="text-muted-foreground">
                Try adjusting your filters or search terms
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function ServiceCard({
  service,
  viewMode,
}: {
  service: (typeof services)[0];
  viewMode: "grid" | "list";
}) {
  const [liked, setLiked] = useState(false);

  if (viewMode === "list") {
    return (
      <Card className="overflow-hidden transition-all hover:shadow-lg">
        <div className="flex">
          <div className="relative h-48 w-64 shrink-0">
            <Image
              src={service.image}
              alt={service.title}
              fill
              className="object-cover"
            />
            {service.isFeatured && (
              <Badge className="absolute left-2 top-2 bg-gradient-to-r from-amber-500 to-orange-500">
                Featured
              </Badge>
            )}
            {service.isNew && (
              <Badge className="absolute left-2 top-2 bg-gradient-to-r from-green-500 to-emerald-500">
                New
              </Badge>
            )}
          </div>
          <div className="flex flex-1 flex-col p-4">
            <div className="flex items-start justify-between">
              <Link href={`/dashboard/marketplace/${service.id}`}>
                <h3 className="font-semibold transition-colors hover:text-primary">
                  {service.title}
                </h3>
              </Link>
              <Button
                variant="ghost"
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
            </div>
            <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">
              {service.description}
            </p>
            <div className="mt-2 flex items-center gap-2">
              <Avatar className="h-6 w-6">
                <AvatarImage src={service.seller.avatar} />
                <AvatarFallback>{service.seller.name[0]}</AvatarFallback>
              </Avatar>
              <span className="text-sm font-medium">{service.seller.name}</span>
              <Badge variant="outline" className="text-xs">
                {service.seller.level}
              </Badge>
            </div>
            <div className="mt-auto flex items-center justify-between pt-4">
              <div className="flex items-center gap-4 text-sm text-muted-foreground">
                <div className="flex items-center gap-1">
                  <Star className="h-4 w-4 fill-amber-400 text-amber-400" />
                  <span className="font-medium text-foreground">
                    {service.rating}
                  </span>
                  <span>({service.reviews})</span>
                </div>
                <div className="flex items-center gap-1">
                  <Clock className="h-4 w-4" />
                  <span>{service.deliveryTime}</span>
                </div>
              </div>
              <div className="text-right">
                <span className="text-sm text-muted-foreground">Starting at</span>
                <p className="text-xl font-bold">${service.price}</p>
              </div>
            </div>
          </div>
        </div>
      </Card>
    );
  }

  return (
    <Card className="group overflow-hidden transition-all hover:shadow-lg">
      <div className="relative aspect-video">
        <Image
          src={service.image}
          alt={service.title}
          fill
          className="object-cover transition-transform duration-300 group-hover:scale-105"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent opacity-0 transition-opacity group-hover:opacity-100" />
        <Button
          variant="ghost"
          size="icon"
          className="absolute right-2 top-2 bg-white/80 backdrop-blur-sm hover:bg-white"
          onClick={() => setLiked(!liked)}
        >
          <Heart
            className={cn("h-4 w-4", liked && "fill-red-500 text-red-500")}
          />
        </Button>
        {service.isFeatured && (
          <Badge className="absolute left-2 top-2 bg-gradient-to-r from-amber-500 to-orange-500">
            Featured
          </Badge>
        )}
        {service.isNew && !service.isFeatured && (
          <Badge className="absolute left-2 top-2 bg-gradient-to-r from-green-500 to-emerald-500">
            New
          </Badge>
        )}
      </div>
      <CardContent className="p-4">
        <div className="flex items-center gap-2">
          <Avatar className="h-6 w-6">
            <AvatarImage src={service.seller.avatar} />
            <AvatarFallback>{service.seller.name[0]}</AvatarFallback>
          </Avatar>
          <span className="text-sm font-medium">{service.seller.name}</span>
          <Badge variant="outline" className="ml-auto text-xs">
            {service.seller.level}
          </Badge>
        </div>
        <Link href={`/dashboard/marketplace/${service.id}`}>
          <h3 className="mt-3 line-clamp-2 font-semibold transition-colors hover:text-primary">
            {service.title}
          </h3>
        </Link>
        <div className="mt-2 flex flex-wrap gap-1">
          {service.tags.map((tag) => (
            <Badge key={tag} variant="secondary" className="text-xs">
              {tag}
            </Badge>
          ))}
        </div>
      </CardContent>
      <CardFooter className="flex items-center justify-between border-t p-4">
        <div className="flex items-center gap-1 text-sm">
          <Star className="h-4 w-4 fill-amber-400 text-amber-400" />
          <span className="font-medium">{service.rating}</span>
          <span className="text-muted-foreground">({service.reviews})</span>
        </div>
        <div className="text-right">
          <span className="text-xs text-muted-foreground">From</span>
          <p className="text-lg font-bold">${service.price}</p>
        </div>
      </CardFooter>
    </Card>
  );
}
