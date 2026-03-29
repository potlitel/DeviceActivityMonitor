import { useState } from 'react'
import { z } from 'zod'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from '@tanstack/react-router'
import { Loader2, LogIn } from 'lucide-react'
import { toast } from 'sonner'
import { authApi } from '@/lib/api'
import { useAuthStore } from '@/stores/auth-store'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { PasswordInput } from '@/components/password-input'

const formSchema = z.object({
  email: z
    .string()
    .min(1, 'Please enter your email')
    .email('Please enter a valid email'),
  password: z
    .string()
    .min(1, 'Please enter your password')
    .min(6, 'Password must be at least 6 characters long'),
})

interface UserAuthFormProps extends React.HTMLAttributes<HTMLFormElement> {
  redirectTo?: string
}

export function UserAuthForm({
  className,
  redirectTo,
  ...props
}: UserAuthFormProps) {
  const [isLoading, setIsLoading] = useState(false)
  const navigate = useNavigate()
  const { setAccessToken, setUser } = useAuthStore()

  const form = useForm<z.infer<typeof formSchema>>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  })

  async function onSubmit(data: z.infer<typeof formSchema>) {
    setIsLoading(true)

    try {
      const response = await authApi.login({
        email: data.email,
        password: data.password,
      })

      // El interceptor ya extrajo "data" de la respuesta
      const responseData = response as unknown as { token: string; userEmail: string }
      
      if (!responseData || !responseData.token) {
        console.error('API Response:', response)
        throw new Error('No token received from server')
      }

      const token = responseData.token
      
      setAccessToken(token)

      // Decodificar el JWT para obtener los datos del usuario
      const parts = token.split('.')
      let payload: Record<string, unknown> = {}
      if (parts.length === 3) {
        try {
          const decoded = atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'))
          payload = JSON.parse(decoded)
        } catch {
          // ignore parse errors
        }
      }

      setUser({
        id: (payload.sub as string) || (payload.user_id as string) || '',
        username: (payload.username as string) || (payload.name as string) || data.email,
        email: data.email,
        role: (payload.role as string) || 'Worker',
      })

      const targetPath = redirectTo || '/'
      navigate({ to: targetPath, replace: true })

      toast.success(`Welcome, ${responseData.userEmail}!`)
    } catch (error: unknown) {
      let message = 'Invalid credentials. Please try again.'
      
      if (typeof error === 'object' && error !== null && 'response' in error) {
        const axiosError = error as { response?: { status?: number; data?: { message?: string } } }
        if (axiosError.response?.status === 401) {
          message = 'Invalid email or password'
        } else if (axiosError.response?.data?.message) {
          message = axiosError.response.data.message
        }
      } else if (error instanceof Error) {
        message = error.message
      }
      
      toast.error(message)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className={cn('grid gap-3', className)}
        {...props}
      >
        <FormField
          control={form.control}
          name='email'
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input placeholder='admin@dam.com' {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name='password'
          render={({ field }) => (
            <FormItem className='relative'>
              <FormLabel>Password</FormLabel>
              <FormControl>
                <PasswordInput placeholder='********' {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button className='mt-2' disabled={isLoading} type='submit'>
          {isLoading ? <Loader2 className='animate-spin' /> : <LogIn />}
          Sign in
        </Button>
      </form>
    </Form>
  )
}
