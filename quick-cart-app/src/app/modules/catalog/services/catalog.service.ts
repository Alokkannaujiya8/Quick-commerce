import { inject, Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';
import { Product, Category } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private readonly api = inject(ApiService);

  private readonly fallbackCategories: Category[] = [
    { id: '1', name: 'All Products', slug: 'all', iconUrl: '🛍️', displayOrder: 1 },
    { id: '2', name: 'Fruits & Vegetables', slug: 'fruits-vegetables', iconUrl: '🥦', displayOrder: 2 },
    { id: '3', name: 'Dairy, Bread & Eggs', slug: 'dairy-bread-eggs', iconUrl: '🥛', displayOrder: 3 },
    { id: '4', name: 'Snacks & Munchies', slug: 'snacks-munchies', iconUrl: '🍿', displayOrder: 4 },
    { id: '5', name: 'Beverages', slug: 'beverages', iconUrl: '🥤', displayOrder: 5 },
    { id: '6', name: 'Instant & Frozen', slug: 'instant-frozen', iconUrl: '🍜', displayOrder: 6 }
  ];

  private readonly fallbackProducts: Product[] = [
    {
      id: 'p1',
      name: 'Fresh Farm Whole Milk',
      slug: 'fresh-farm-whole-milk',
      description: 'Pasteurized whole milk rich in calcium and vitamins.',
      sku: 'MILK-001',
      unitOfMeasure: '500 ml',
      price: 34,
      originalPrice: 38,
      isActive: true,
      subCategoryId: 'sc1',
      categoryName: 'dairy-bread-eggs',
      imageUrl: 'https://images.unsplash.com/photo-1550583724-b2692b85b150?auto=format&fit=crop&w=400&q=80'
    },
    {
      id: 'p2',
      name: 'Artisan Whole Wheat Bread',
      slug: 'artisan-whole-wheat-bread',
      description: '100% whole grain brown bread freshly baked daily.',
      sku: 'BREAD-002',
      unitOfMeasure: '400 g',
      price: 45,
      originalPrice: 50,
      isActive: true,
      subCategoryId: 'sc1',
      categoryName: 'dairy-bread-eggs',
      imageUrl: 'https://images.unsplash.com/photo-1509440159596-0249088772ff?auto=format&fit=crop&w=400&q=80'
    },
    {
      id: 'p3',
      name: 'Farm Fresh Brown Eggs (Pack of 6)',
      slug: 'fresh-brown-eggs-6',
      description: 'Protein-rich organic brown eggs sourced directly from farms.',
      sku: 'EGGS-003',
      unitOfMeasure: '6 pcs',
      price: 58,
      originalPrice: 65,
      isActive: true,
      subCategoryId: 'sc1',
      categoryName: 'dairy-bread-eggs',
      imageUrl: 'https://images.unsplash.com/photo-1516448620398-c5f44bf9f441?auto=format&fit=crop&w=400&q=80'
    },
    {
      id: 'p4',
      name: 'Fresh Crisp Broccoli',
      slug: 'fresh-crisp-broccoli',
      description: 'Nutrient-rich green broccoli directly from local farms.',
      sku: 'VEG-004',
      unitOfMeasure: '250 g',
      price: 42,
      originalPrice: 55,
      isActive: true,
      subCategoryId: 'sc2',
      categoryName: 'fruits-vegetables',
      imageUrl: 'https://images.unsplash.com/photo-1584270354949-c26b0d5b4a0c?auto=format&fit=crop&w=400&q=80'
    },
    {
      id: 'p5',
      name: 'Shimla Fresh Red Apples',
      slug: 'shimla-red-apples',
      description: 'Sweet, juicy and crunchy hand-picked Shimla apples.',
      sku: 'FRUIT-005',
      unitOfMeasure: '500 g (3-4 pcs)',
      price: 110,
      originalPrice: 135,
      isActive: true,
      subCategoryId: 'sc2',
      categoryName: 'fruits-vegetables',
      imageUrl: 'https://images.unsplash.com/photo-1560806887-1e4cd0b6cbd6?auto=format&fit=crop&w=400&q=80'
    },
    {
      id: 'p6',
      name: 'Classic Salted Potato Chips',
      slug: 'classic-salted-chips',
      description: 'Thinly sliced crispy salted potato wafers.',
      sku: 'SNACK-006',
      unitOfMeasure: '115 g',
      price: 30,
      originalPrice: 30,
      isActive: true,
      subCategoryId: 'sc3',
      categoryName: 'snacks-munchies',
      imageUrl: 'https://images.unsplash.com/photo-1566478989037-eec170784d0b?auto=format&fit=crop&w=400&q=80'
    },
    {
      id: 'p7',
      name: 'Cold Pressed Orange Juice',
      slug: 'cold-pressed-orange-juice',
      description: '100% pure Valencia orange juice with zero added sugar.',
      sku: 'BEV-007',
      unitOfMeasure: '300 ml',
      price: 79,
      originalPrice: 99,
      isActive: true,
      subCategoryId: 'sc4',
      categoryName: 'beverages',
      imageUrl: 'https://images.unsplash.com/photo-1613478223719-2ab802602423?auto=format&fit=crop&w=400&q=80'
    },
    {
      id: 'p8',
      name: 'Spicy Masala Instant Noodles (Pack of 4)',
      slug: 'masala-instant-noodles-pack',
      description: 'Quick-cook savory masala noodles ready in 2 minutes.',
      sku: 'INST-008',
      unitOfMeasure: '280 g',
      price: 56,
      originalPrice: 60,
      isActive: true,
      subCategoryId: 'sc5',
      categoryName: 'instant-frozen',
      imageUrl: 'https://images.unsplash.com/photo-1612927601601-6638404737ce?auto=format&fit=crop&w=400&q=80'
    }
  ];

  getCategories(): Observable<Category[]> {
    return this.api.get<Category[]>('catalog/categories').pipe(
      catchError(() => of(this.fallbackCategories))
    );
  }

  getProducts(categorySlug?: string, search?: string): Observable<Product[]> {
    return this.api.get<Product[]>('catalog/products', { category: categorySlug ?? '', q: search ?? '' }).pipe(
      catchError(() => {
        let filtered = this.fallbackProducts;
        if (categorySlug && categorySlug !== 'all') {
          filtered = filtered.filter((p) => p.categoryName === categorySlug);
        }
        if (search) {
          filtered = filtered.filter((p) =>
            p.name.toLowerCase().includes(search.toLowerCase())
          );
        }
        return of(filtered);
      })
    );
  }

  getProductBySlug(slug: string): Observable<Product | null> {
    return this.api.get<Product>(`catalog/products/${slug}`).pipe(
      catchError(() => {
        const found = this.fallbackProducts.find((p) => p.slug === slug) ?? null;
        return of(found);
      })
    );
  }
}
