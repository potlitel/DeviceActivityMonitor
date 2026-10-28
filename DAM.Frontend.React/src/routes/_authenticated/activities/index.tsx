import { createFileRoute } from '@tanstack/react-router'
import { z } from 'zod'
import { Activities } from '@/features/activities'

export const Route = createFileRoute('/_authenticated/activities/')({
  validateSearch: z.object({
    pageNumber: z.coerce.number().default(1),
    pageSize: z.coerce.number().default(10),
    serial: z.string().optional(),
  }),
  component: Activities,
})
