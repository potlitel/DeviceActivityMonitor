import { createFileRoute } from '@tanstack/react-router'
import { z } from 'zod'
import { Invoices } from '@/features/invoices'

export const Route = createFileRoute('/_authenticated/invoices/')({
  validateSearch: z.object({
    pageNumber: z.coerce.number().default(1),
    pageSize: z.coerce.number().default(10),
  }),
  component: Invoices,
})
