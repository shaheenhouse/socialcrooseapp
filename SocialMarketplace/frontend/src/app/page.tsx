'use client';

import { motion } from 'framer-motion';
import Link from 'next/link';
import { 
  ArrowRight, 
  Briefcase, 
  Building2, 
  CheckCircle, 
  Globe, 
  Shield, 
  Sparkles, 
  Star,
  TrendingUp,
  Users,
  Zap
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Navbar } from '@/components/layout/navbar';
import { Footer } from '@/components/layout/footer';

const features = [
  {
    icon: Users,
    title: 'Multi-Role Platform',
    description: 'Support for freelancers, businesses, stores, agencies, and government entities with customizable roles and permissions.',
  },
  {
    icon: Shield,
    title: 'Secure Escrow Payments',
    description: 'Protected transactions with milestone-based releases and dispute resolution for peace of mind.',
  },
  {
    icon: CheckCircle,
    title: 'Verified Skills',
    description: 'Take skill tests, earn certificates, and showcase your verified expertise to potential clients.',
  },
  {
    icon: Globe,
    title: 'Multi-Language Support',
    description: 'Use the platform in your preferred language with comprehensive internationalization.',
  },
  {
    icon: Building2,
    title: 'Government Tenders',
    description: 'Bid on government projects and tenders with a transparent procurement process.',
  },
  {
    icon: TrendingUp,
    title: 'Project Management',
    description: 'Complete project lifecycle management with milestones, budgets, and Gantt charts.',
  },
];

const stats = [
  { value: '50K+', label: 'Active Users' },
  { value: '10K+', label: 'Projects Completed' },
  { value: '$2M+', label: 'Transactions' },
  { value: '98%', label: 'Satisfaction Rate' },
];

export default function HomePage() {
  return (
    <div className="min-h-screen flex flex-col">
      <Navbar />
      
      {/* Hero Section */}
      <section className="relative overflow-hidden pt-20 pb-32">
        {/* Gradient Background */}
        <div className="absolute inset-0 bg-gradient-to-br from-primary/5 via-transparent to-purple-500/5" />
        <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[800px] bg-primary/10 rounded-full blur-3xl opacity-50" />
        
        <div className="container relative">
          <motion.div 
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            className="text-center max-w-4xl mx-auto"
          >
            <Badge variant="secondary" className="mb-6 px-4 py-2">
              <Sparkles className="w-4 h-4 mr-2" />
              The Future of Work is Here
            </Badge>
            
            <h1 className="text-5xl md:text-7xl font-bold tracking-tight mb-6">
              Connect, Trade,{' '}
              <span className="gradient-text">Grow Together</span>
            </h1>
            
            <p className="text-xl text-muted-foreground mb-8 max-w-2xl mx-auto">
              A comprehensive social marketplace connecting freelancers, businesses, and government entities. 
              Find talent, sell products, bid on projects, and grow your business.
            </p>
            
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Button size="lg" asChild className="gap-2">
                <Link href="/auth/register">
                  Get Started Free
                  <ArrowRight className="w-4 h-4" />
                </Link>
              </Button>
              <Button size="lg" variant="outline" asChild>
                <Link href="/marketplace">
                  Explore Marketplace
                </Link>
              </Button>
            </div>
          </motion.div>
          
          {/* Stats */}
          <motion.div 
            initial={{ opacity: 0, y: 40 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.2 }}
            className="mt-20 grid grid-cols-2 md:grid-cols-4 gap-8"
          >
            {stats.map((stat, index) => (
              <div key={index} className="text-center">
                <div className="text-4xl md:text-5xl font-bold gradient-text">{stat.value}</div>
                <div className="text-muted-foreground mt-1">{stat.label}</div>
              </div>
            ))}
          </motion.div>
        </div>
      </section>
      
      {/* Features Section */}
      <section className="py-24 bg-muted/30">
        <div className="container">
          <motion.div 
            initial={{ opacity: 0 }}
            whileInView={{ opacity: 1 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <Badge variant="outline" className="mb-4">Features</Badge>
            <h2 className="text-4xl font-bold mb-4">Everything You Need</h2>
            <p className="text-muted-foreground max-w-2xl mx-auto">
              A complete suite of tools for modern professionals and businesses.
            </p>
          </motion.div>
          
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            {features.map((feature, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
              >
                <Card className="h-full hover:shadow-lg transition-all duration-300 hover:-translate-y-1 border-border/50 bg-card/50 backdrop-blur">
                  <CardHeader>
                    <div className="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center mb-4">
                      <feature.icon className="w-6 h-6 text-primary" />
                    </div>
                    <CardTitle>{feature.title}</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <CardDescription className="text-base">{feature.description}</CardDescription>
                  </CardContent>
                </Card>
              </motion.div>
            ))}
          </div>
        </div>
      </section>
      
      {/* CTA Section */}
      <section className="py-24">
        <div className="container">
          <motion.div 
            initial={{ opacity: 0, scale: 0.95 }}
            whileInView={{ opacity: 1, scale: 1 }}
            viewport={{ once: true }}
            className="relative overflow-hidden rounded-3xl bg-gradient-to-br from-primary to-purple-600 p-12 md:p-20 text-center text-white"
          >
            <div className="absolute inset-0 bg-[url('/grid.svg')] opacity-10" />
            <div className="relative">
              <Zap className="w-12 h-12 mx-auto mb-6" />
              <h2 className="text-4xl md:text-5xl font-bold mb-4">Ready to Get Started?</h2>
              <p className="text-xl opacity-90 mb-8 max-w-2xl mx-auto">
                Join thousands of professionals and businesses already growing on our platform.
              </p>
              <Button size="lg" variant="secondary" asChild className="gap-2">
                <Link href="/auth/register">
                  Create Your Account
                  <ArrowRight className="w-4 h-4" />
                </Link>
              </Button>
            </div>
          </motion.div>
        </div>
      </section>
      
      <Footer />
    </div>
  );
}
