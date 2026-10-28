import { createFileRoute } from '@tanstack/react-router'
import { z } from 'zod'
import { AuditLogs } from '@/features/audit'

export const Route = createFileRoute('/_authenticated/audit/')({
  validateSearch: z.object({
    pageNumber: z.coerce.number().default(1),
    pageSize: z.coerce.number().default(10),
  }),
  component: AuditLogs,
})
