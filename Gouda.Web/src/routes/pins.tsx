import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/pins')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/pins"!</div>
}
