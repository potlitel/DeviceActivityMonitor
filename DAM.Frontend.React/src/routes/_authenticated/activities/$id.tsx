import { createFileRoute } from '@tanstack/react-router'
import { ActivityDetail } from '@/features/activities/$id'

export const Route = createFileRoute('/_authenticated/activities/$id')({
  component: ActivityDetail,
})
