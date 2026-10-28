import { createFileRoute } from '@tanstack/react-router'
import { z } from 'zod'
import { Presence } from '@/features/presence'

export const Route = createFileRoute('/_authenticated/presence/')({
  validateSearch: z.object({
    pageNumber: z.coerce.number().default(1),
    pageSize: z.coerce.number().default(10),
  }),
  component: Presence,
})
