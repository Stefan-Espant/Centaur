export interface FieldDefinitionDto {
  id: string
  name: string
  slug: string
  type: string
  config: Record<string, unknown>
}

export interface BlockTypeDto {
  id: string
  name: string
  slug: string
  fields: FieldDefinitionDto[]
  createdAt: string
  updatedAt: string
}

export interface CreateBlockTypeRequest {
  name: string
  slug: string
  fields: FieldDefinitionDto[]
}

export interface UpdateBlockTypeRequest {
  name: string
  fields: FieldDefinitionDto[]
}

export function useBlockTypes() {
  const { get, post, put, del } = useApi()

  async function getAll(): Promise<BlockTypeDto[]> {
    return await get<BlockTypeDto[]>('/api/admin/block-types')
  }

  async function getBySlug(slug: string): Promise<BlockTypeDto> {
    return await get<BlockTypeDto>(`/api/admin/block-types/${slug}`)
  }

  async function create(request: CreateBlockTypeRequest): Promise<BlockTypeDto> {
    return await post<BlockTypeDto>('/api/admin/block-types', request)
  }

  async function update(slug: string, request: UpdateBlockTypeRequest): Promise<BlockTypeDto> {
    return await put<BlockTypeDto>(`/api/admin/block-types/${slug}`, request)
  }

  async function remove(slug: string): Promise<void> {
    await del(`/api/admin/block-types/${slug}`)
  }

  return { getAll, getBySlug, create, update, remove }
}
