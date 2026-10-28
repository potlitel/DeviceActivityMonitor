import { createFileRoute } from '@tanstack/react-router'
import { z } from 'zod'
import { SystemEvents } from '@/features/system-events'

export const Route = createFileRoute('/_authenticated/system-events/')({
  validateSearch: z.object({
    pageNumber: z.coerce.number().default(1),
    pageSize: z.coerce.number().default(10),
  }),
  component: SystemEvents,
})
