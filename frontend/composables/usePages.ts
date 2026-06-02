export interface BlockInstance extends Record<string, unknown> {
  _type: string
  _id: string
}

export interface PageDto {
  id: string
  title: string
  slug: string
  metaDescription: string
  body: BlockInstance[]
  createdAt: string
  updatedAt: string
  status: string | null
}

export interface SavePageRequest {
  title: string
  slug: string
  metaDescription: string
  body: BlockInstance[]
  status: string | null
}

export function usePages() {
  const { get, post, put, del } = useApi()

  async function getAll(): Promise<PageDto[]> {
    return await get<PageDto[]>('/api/pages')
  }

  async function getById(id: string): Promise<PageDto> {
    return await get<PageDto>(`/api/pages/${id}`)
  }

  async function create(request: SavePageRequest): Promise<PageDto> {
    return await post<PageDto>('/api/pages', request)
  }

  async function update(id: string, request: SavePageRequest): Promise<PageDto> {
    return await put<PageDto>(`/api/pages/${id}`, request)
  }

  async function remove(id: string): Promise<void> {
    await del(`/api/pages/${id}`)
  }

  return { getAll, getById, create, update, remove }
}
