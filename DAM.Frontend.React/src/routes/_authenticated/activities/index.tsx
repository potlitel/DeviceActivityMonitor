import { createFileRoute } from '@tanstack/react-router'
import { Activities } from '@/features/activities'

export const Route = createFileRoute('/_authenticated/activities/')({
  component: Activities,
})
