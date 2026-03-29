import { createFileRoute } from '@tanstack/react-router'
import { SystemEvents } from '@/features/system-events'

export const Route = createFileRoute('/_authenticated/system-events/')({
  component: SystemEvents,
})
