"use client";

import { useState, useEffect, useCallback } from "react";
import { useRouter } from "next/navigation";
import { motion, AnimatePresence } from "framer-motion";
import {
  Search,
  User,
  Briefcase,
  Building2,
  FileText,
  ShoppingBag,
  Clock,
  TrendingUp,
  X,
  ArrowRight,
} from "lucide-react";
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from "@/components/ui/command";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

// Mock data for search suggestions
const recentSearches = [
  { query: "React developer", type: "people" },
  { query: "Logo design", type: "services" },
  { query: "E-commerce project", type: "projects" },
];

const trendingSearches = [
  { query: "AI/ML Engineer", count: 1250 },
  { query: "Mobile App Development", count: 980 },
  { query: "UI/UX Design", count: 870 },
  { query: "Full Stack Developer", count: 760 },
];

const quickResults = {
  people: [
    {
      id: "1",
      name: "Sarah Johnson",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=sarah",
      title: "Senior Full Stack Developer",
      connections: 248,
    },
    {
      id: "2",
      name: "Ahmed Khan",
      avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=ahmed",
      title: "Product Manager",
      connections: 512,
    },
  ],
  services: [
    {
      id: "1",
      title: "Professional Logo Design",
      seller: "DesignStudio",
      price: "$50",
      rating: 4.9,
    },
    {
      id: "2",
      title: "Full Website Development",
      seller: "WebExperts",
      price: "$500",
      rating: 4.8,
    },
  ],
  companies: [
    {
      id: "1",
      name: "TechCorp Inc.",
      logo: "https://api.dicebear.com/7.x/shapes/svg?seed=techcorp",
      industry: "Technology",
      followers: 15000,
    },
  ],
};

const searchTypeIcons: Record<string, React.ReactNode> = {
  people: <User className="h-4 w-4" />,
  services: <ShoppingBag className="h-4 w-4" />,
  projects: <Briefcase className="h-4 w-4" />,
  companies: <Building2 className="h-4 w-4" />,
  posts: <FileText className="h-4 w-4" />,
};

interface GlobalSearchProps {
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
}

export function GlobalSearch({ open, onOpenChange }: GlobalSearchProps) {
  const router = useRouter();
  const [query, setQuery] = useState("");
  const [isOpen, setIsOpen] = useState(open ?? false);

  useEffect(() => {
    if (open !== undefined) {
      setIsOpen(open);
    }
  }, [open]);

  useEffect(() => {
    const down = (e: KeyboardEvent) => {
      if (e.key === "k" && (e.metaKey || e.ctrlKey)) {
        e.preventDefault();
        setIsOpen((prev) => !prev);
        onOpenChange?.(!isOpen);
      }
    };

    document.addEventListener("keydown", down);
    return () => document.removeEventListener("keydown", down);
  }, [isOpen, onOpenChange]);

  const handleOpenChange = (newOpen: boolean) => {
    setIsOpen(newOpen);
    onOpenChange?.(newOpen);
    if (!newOpen) {
      setQuery("");
    }
  };

  const handleSearch = useCallback(
    (searchQuery: string, type?: string) => {
      const params = new URLSearchParams({ q: searchQuery });
      if (type) params.set("type", type);
      router.push(`/dashboard/search?${params.toString()}`);
      handleOpenChange(false);
    },
    [router]
  );

  return (
    <CommandDialog open={isOpen} onOpenChange={handleOpenChange}>
      <div className="flex items-center border-b px-3">
        <Search className="mr-2 h-4 w-4 shrink-0 opacity-50" />
        <input
          placeholder="Search people, services, projects, companies..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === "Enter" && query.trim()) {
              handleSearch(query);
            }
          }}
          className="flex h-11 w-full rounded-md bg-transparent py-3 text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50"
        />
        {query && (
          <button
            onClick={() => setQuery("")}
            className="p-1 hover:bg-muted rounded"
          >
            <X className="h-4 w-4" />
          </button>
        )}
        <kbd className="pointer-events-none ml-2 hidden h-5 select-none items-center gap-1 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium opacity-100 sm:flex">
          <span className="text-xs">⌘</span>K
        </kbd>
      </div>

      <CommandList className="max-h-[400px]">
        {!query && (
          <>
            {/* Recent Searches */}
            <CommandGroup heading="Recent">
              {recentSearches.map((search, index) => (
                <CommandItem
                  key={index}
                  onSelect={() => handleSearch(search.query, search.type)}
                  className="flex items-center gap-3 cursor-pointer"
                >
                  <Clock className="h-4 w-4 text-muted-foreground" />
                  <span>{search.query}</span>
                  <Badge variant="secondary" className="ml-auto text-xs">
                    {search.type}
                  </Badge>
                </CommandItem>
              ))}
            </CommandGroup>

            <CommandSeparator />

            {/* Trending Searches */}
            <CommandGroup heading="Trending">
              {trendingSearches.map((search, index) => (
                <CommandItem
                  key={index}
                  onSelect={() => handleSearch(search.query)}
                  className="flex items-center gap-3 cursor-pointer"
                >
                  <TrendingUp className="h-4 w-4 text-orange-500" />
                  <span>{search.query}</span>
                  <span className="ml-auto text-xs text-muted-foreground">
                    {search.count.toLocaleString()} searches
                  </span>
                </CommandItem>
              ))}
            </CommandGroup>
          </>
        )}

        {query && (
          <>
            {/* Quick search by type */}
            <CommandGroup heading="Search in">
              {Object.entries(searchTypeIcons).map(([type, icon]) => (
                <CommandItem
                  key={type}
                  onSelect={() => handleSearch(query, type)}
                  className="flex items-center gap-3 cursor-pointer"
                >
                  {icon}
                  <span>Search &ldquo;{query}&rdquo; in {type}</span>
                  <ArrowRight className="ml-auto h-4 w-4 text-muted-foreground" />
                </CommandItem>
              ))}
            </CommandGroup>

            <CommandSeparator />

            {/* Quick Results - People */}
            <CommandGroup heading="People">
              {quickResults.people.map((person) => (
                <CommandItem
                  key={person.id}
                  onSelect={() => router.push(`/profile/${person.id}`)}
                  className="flex items-center gap-3 cursor-pointer"
                >
                  <Avatar className="h-8 w-8">
                    <AvatarImage src={person.avatar} />
                    <AvatarFallback>{person.name[0]}</AvatarFallback>
                  </Avatar>
                  <div className="flex-1">
                    <p className="text-sm font-medium">{person.name}</p>
                    <p className="text-xs text-muted-foreground">{person.title}</p>
                  </div>
                  <span className="text-xs text-muted-foreground">
                    {person.connections} connections
                  </span>
                </CommandItem>
              ))}
            </CommandGroup>

            {/* Quick Results - Services */}
            <CommandGroup heading="Services">
              {quickResults.services.map((service) => (
                <CommandItem
                  key={service.id}
                  onSelect={() => router.push(`/marketplace/${service.id}`)}
                  className="flex items-center gap-3 cursor-pointer"
                >
                  <ShoppingBag className="h-8 w-8 p-1.5 bg-primary/10 rounded" />
                  <div className="flex-1">
                    <p className="text-sm font-medium">{service.title}</p>
                    <p className="text-xs text-muted-foreground">
                      by {service.seller}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-medium">{service.price}</p>
                    <p className="text-xs text-muted-foreground">
                      ⭐ {service.rating}
                    </p>
                  </div>
                </CommandItem>
              ))}
            </CommandGroup>

            {/* Quick Results - Companies */}
            <CommandGroup heading="Companies">
              {quickResults.companies.map((company) => (
                <CommandItem
                  key={company.id}
                  onSelect={() => router.push(`/company/${company.id}`)}
                  className="flex items-center gap-3 cursor-pointer"
                >
                  <Avatar className="h-8 w-8 rounded">
                    <AvatarImage src={company.logo} />
                    <AvatarFallback>{company.name[0]}</AvatarFallback>
                  </Avatar>
                  <div className="flex-1">
                    <p className="text-sm font-medium">{company.name}</p>
                    <p className="text-xs text-muted-foreground">
                      {company.industry}
                    </p>
                  </div>
                  <span className="text-xs text-muted-foreground">
                    {company.followers.toLocaleString()} followers
                  </span>
                </CommandItem>
              ))}
            </CommandGroup>
          </>
        )}

        <CommandEmpty>
          <div className="py-6 text-center text-sm">
            <p className="text-muted-foreground mb-2">No results found</p>
            <button
              onClick={() => handleSearch(query)}
              className="text-primary hover:underline"
            >
              Search all for &ldquo;{query}&rdquo;
            </button>
          </div>
        </CommandEmpty>
      </CommandList>
    </CommandDialog>
  );
}

export function SearchTrigger({ className }: { className?: string }) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        className={cn(
          "flex items-center gap-2 h-9 w-full max-w-sm rounded-md border border-input bg-background px-3 text-sm text-muted-foreground ring-offset-background transition-colors hover:bg-accent hover:text-accent-foreground",
          className
        )}
      >
        <Search className="h-4 w-4" />
        <span className="hidden sm:inline-flex">Search...</span>
        <kbd className="pointer-events-none ml-auto hidden h-5 select-none items-center gap-1 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium opacity-100 sm:flex">
          <span className="text-xs">⌘</span>K
        </kbd>
      </button>
      <GlobalSearch open={open} onOpenChange={setOpen} />
    </>
  );
}
