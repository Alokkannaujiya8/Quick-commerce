export interface Product {
  id: string;
  name: string;
  slug: string;
  description?: string;
  sku: string;
  unitOfMeasure: string;
  imageUrl?: string;
  price: number;
  originalPrice?: number;
  isActive: boolean;
  brandId?: string;
  brandName?: string;
  subCategoryId: string;
  categoryName?: string;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  iconUrl?: string;
  displayOrder: number;
}

export interface SubCategory {
  id: string;
  categoryId: string;
  name: string;
  slug: string;
  displayOrder: number;
}

export interface Brand {
  id: string;
  name: string;
  logoUrl?: string;
}
