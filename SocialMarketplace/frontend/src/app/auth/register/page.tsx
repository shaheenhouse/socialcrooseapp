'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { motion } from 'framer-motion';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Eye, EyeOff, Loader2, ArrowLeft, Check } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useAuthStore } from '@/store/auth-store';

const registerSchema = z.object({
  firstName: z.string().min(2, 'First name must be at least 2 characters'),
  lastName: z.string().min(2, 'Last name must be at least 2 characters'),
  username: z.string().min(3, 'Username must be at least 3 characters').regex(/^[a-zA-Z0-9_]+$/, 'Username can only contain letters, numbers, and underscores'),
  email: z.string().email('Please enter a valid email'),
  password: z.string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
    .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
    .regex(/[0-9]/, 'Password must contain at least one number'),
  confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, {
  message: "Passwords don't match",
  path: ["confirmPassword"],
});

type RegisterForm = z.infer<typeof registerSchema>;

const passwordRequirements = [
  { regex: /.{8,}/, label: 'At least 8 characters' },
  { regex: /[A-Z]/, label: 'One uppercase letter' },
  { regex: /[a-z]/, label: 'One lowercase letter' },
  { regex: /[0-9]/, label: 'One number' },
];

export default function RegisterPage() {
  const router = useRouter();
  const [showPassword, setShowPassword] = useState(false);
  const { register: registerUser, isLoading, error } = useAuthStore();
  
  const form = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
    },
  });

  const password = form.watch('password');

  const onSubmit = async (data: RegisterForm) => {
    try {
      await registerUser({
        firstName: data.firstName,
        lastName: data.lastName,
        username: data.username,
        email: data.email,
        password: data.password,
      });
      toast.success('Account created successfully!');
      router.push('/dashboard');
    } catch (error) {
      toast.error('Registration failed. Please try again.');
    }
  };

  return (
    <div className="min-h-screen flex">
      {/* Left Side - Gradient Background */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-primary via-purple-600 to-pink-500 relative overflow-hidden">
        <div className="absolute inset-0 bg-[url('/grid.svg')] opacity-20" />
        <div className="relative z-10 flex flex-col justify-center p-12 text-white">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <Link href="/" className="flex items-center gap-2 mb-12">
              <div className="w-10 h-10 rounded-xl bg-white/20 flex items-center justify-center">
                <span className="text-white font-bold text-xl">M</span>
              </div>
              <span className="font-bold text-2xl">Marketplace</span>
            </Link>
            
            <h1 className="text-4xl font-bold mb-4">Join Our Community</h1>
            <p className="text-xl text-white/80 mb-8">
              Create your account and start connecting with professionals, finding projects, and growing your business.
            </p>
            
            <div className="space-y-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center">
                  ✓
                </div>
                <span>Free to join and easy to use</span>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center">
                  ✓
                </div>
                <span>Secure escrow payments</span>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center">
                  ✓
                </div>
                <span>Verified skills and certifications</span>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center">
                  ✓
                </div>
                <span>Access to government tenders</span>
              </div>
            </div>
          </motion.div>
        </div>
      </div>

      {/* Right Side - Register Form */}
      <div className="w-full lg:w-1/2 flex items-center justify-center p-8 overflow-y-auto">
        <motion.div
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.6 }}
          className="w-full max-w-md"
        >
          <Link 
            href="/" 
            className="inline-flex items-center text-sm text-muted-foreground hover:text-foreground mb-8 lg:hidden"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to home
          </Link>

          <Card className="border-0 shadow-xl">
            <CardHeader className="space-y-1 text-center">
              <CardTitle className="text-3xl font-bold">Create Account</CardTitle>
              <CardDescription>
                Fill in your details to get started
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <label className="text-sm font-medium">First Name</label>
                    <Input
                      placeholder="John"
                      {...form.register('firstName')}
                      className={form.formState.errors.firstName ? 'border-destructive' : ''}
                    />
                    {form.formState.errors.firstName && (
                      <p className="text-xs text-destructive">{form.formState.errors.firstName.message}</p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Last Name</label>
                    <Input
                      placeholder="Doe"
                      {...form.register('lastName')}
                      className={form.formState.errors.lastName ? 'border-destructive' : ''}
                    />
                    {form.formState.errors.lastName && (
                      <p className="text-xs text-destructive">{form.formState.errors.lastName.message}</p>
                    )}
                  </div>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Username</label>
                  <Input
                    placeholder="johndoe"
                    {...form.register('username')}
                    className={form.formState.errors.username ? 'border-destructive' : ''}
                  />
                  {form.formState.errors.username && (
                    <p className="text-xs text-destructive">{form.formState.errors.username.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Email</label>
                  <Input
                    type="email"
                    placeholder="you@example.com"
                    {...form.register('email')}
                    className={form.formState.errors.email ? 'border-destructive' : ''}
                  />
                  {form.formState.errors.email && (
                    <p className="text-xs text-destructive">{form.formState.errors.email.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Password</label>
                  <div className="relative">
                    <Input
                      type={showPassword ? 'text' : 'password'}
                      placeholder="••••••••"
                      {...form.register('password')}
                      className={form.formState.errors.password ? 'border-destructive pr-10' : 'pr-10'}
                    />
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="absolute right-0 top-0 h-full px-3 hover:bg-transparent"
                      onClick={() => setShowPassword(!showPassword)}
                    >
                      {showPassword ? (
                        <EyeOff className="h-4 w-4 text-muted-foreground" />
                      ) : (
                        <Eye className="h-4 w-4 text-muted-foreground" />
                      )}
                    </Button>
                  </div>
                  
                  {/* Password Requirements */}
                  <div className="mt-2 space-y-1">
                    {passwordRequirements.map((req, index) => (
                      <div key={index} className="flex items-center gap-2 text-xs">
                        <Check 
                          className={`w-3 h-3 ${
                            req.regex.test(password) 
                              ? 'text-success' 
                              : 'text-muted-foreground'
                          }`} 
                        />
                        <span className={
                          req.regex.test(password) 
                            ? 'text-success' 
                            : 'text-muted-foreground'
                        }>
                          {req.label}
                        </span>
                      </div>
                    ))}
                  </div>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Confirm Password</label>
                  <Input
                    type="password"
                    placeholder="••••••••"
                    {...form.register('confirmPassword')}
                    className={form.formState.errors.confirmPassword ? 'border-destructive' : ''}
                  />
                  {form.formState.errors.confirmPassword && (
                    <p className="text-xs text-destructive">{form.formState.errors.confirmPassword.message}</p>
                  )}
                </div>

                {error && (
                  <div className="p-3 rounded-lg bg-destructive/10 text-destructive text-sm">
                    {error}
                  </div>
                )}

                <p className="text-xs text-muted-foreground">
                  By creating an account, you agree to our{' '}
                  <Link href="/terms" className="text-primary hover:underline">Terms of Service</Link>
                  {' '}and{' '}
                  <Link href="/privacy" className="text-primary hover:underline">Privacy Policy</Link>.
                </p>

                <Button type="submit" className="w-full" size="lg" disabled={isLoading}>
                  {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Create Account
                </Button>

                <p className="text-center text-sm text-muted-foreground mt-6">
                  Already have an account?{' '}
                  <Link href="/auth/login" className="text-primary hover:underline font-medium">
                    Sign in
                  </Link>
                </p>
              </form>
            </CardContent>
          </Card>
        </motion.div>
      </div>
    </div>
  );
}
