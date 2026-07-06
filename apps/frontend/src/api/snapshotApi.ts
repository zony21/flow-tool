import { httpClient } from './httpClient'

export async function createSnapshot(flowId: string): Promise<void> {
  await httpClient.post(`/api/flows/${flowId}/snapshots`)
}
