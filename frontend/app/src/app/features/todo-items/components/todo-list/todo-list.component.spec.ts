import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { vi } from 'vitest';
import { TodoListComponent } from './todo-list.component';
import { TodoItemsClient, TodoItemDto } from '../../../../core/api/api';
import { ErrorService } from '../../../../core/services/error.service';

describe('TodoListComponent', () => {
  let fixture: ComponentFixture<TodoListComponent>;
  let component: TodoListComponent;
  let client: Pick<TodoItemsClient, 'getTodoItems' | 'updateTodoItem' | 'deleteTodoItem'>;
  let errorService: Pick<ErrorService, 'extractMessage'>;

  const mockItems: TodoItemDto[] = [
    { id: '1', title: 'Item 1', description: null, isCompleted: false, createdAt: '', completedAt: null },
    { id: '2', title: 'Item 2', description: 'desc', isCompleted: true, createdAt: '', completedAt: '' },
  ];

  beforeEach(() => {
    client = { getTodoItems: vi.fn(), updateTodoItem: vi.fn(), deleteTodoItem: vi.fn() };
    errorService = { extractMessage: vi.fn() };
    (client.getTodoItems as ReturnType<typeof vi.fn>).mockReturnValue(of(mockItems));

    TestBed.configureTestingModule({
      imports: [TodoListComponent],
      providers: [
        provideRouter([]),
        { provide: TodoItemsClient, useValue: client },
        { provide: ErrorService, useValue: errorService },
      ],
    });

    fixture = TestBed.createComponent(TodoListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should load items on init', () => {
    expect(component.items()).toEqual(mockItems);
    expect(component.isLoading()).toBe(false);
  });

  it('should compute pending item count correctly', () => {
    expect(component.pendingItemCount()).toBe(1);
  });

  it('should set errorMessage when load fails', () => {
    (client.getTodoItems as ReturnType<typeof vi.fn>).mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 }))
    );
    (errorService.extractMessage as ReturnType<typeof vi.fn>).mockReturnValue('Server error');

    component.loadItems();

    expect(component.errorMessage()).toBe('Server error');
    expect(component.isLoading()).toBe(false);
  });

  it('should update item in the list after toggling completion', () => {
    const updated: TodoItemDto = { ...mockItems[0], isCompleted: true };
    (client.updateTodoItem as ReturnType<typeof vi.fn>).mockReturnValue(of(updated));

    component.toggleComplete(mockItems[0]);

    expect(component.items()[0].isCompleted).toBe(true);
  });

  it('should set errorMessage when toggle fails', () => {
    (client.updateTodoItem as ReturnType<typeof vi.fn>).mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 }))
    );
    (errorService.extractMessage as ReturnType<typeof vi.fn>).mockReturnValue('Update failed');

    component.toggleComplete(mockItems[0]);

    expect(component.errorMessage()).toBe('Update failed');
  });

  it('should remove item from list after successful delete', () => {
    (client.deleteTodoItem as ReturnType<typeof vi.fn>).mockReturnValue(of(undefined));

    component.delete('1');

    expect(component.items().length).toBe(1);
    expect(component.items()[0].id).toBe('2');
  });

  it('should set errorMessage when delete fails', () => {
    (client.deleteTodoItem as ReturnType<typeof vi.fn>).mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 }))
    );
    (errorService.extractMessage as ReturnType<typeof vi.fn>).mockReturnValue('Delete failed');

    component.delete('1');

    expect(component.errorMessage()).toBe('Delete failed');
  });
});

