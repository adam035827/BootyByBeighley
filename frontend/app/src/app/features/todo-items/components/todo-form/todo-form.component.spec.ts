import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { vi } from 'vitest';
import { TodoFormComponent } from './todo-form.component';
import { TodoItemsClient, TodoItemDto } from '../../../../core/api/api';
import { ErrorService } from '../../../../core/services/error.service';

describe('TodoFormComponent (create mode)', () => {
  let fixture: ComponentFixture<TodoFormComponent>;
  let component: TodoFormComponent;
  let client: Pick<TodoItemsClient, 'getTodoItem' | 'createTodoItem' | 'updateTodoItem'>;
  let errorService: Pick<ErrorService, 'extractMessage'>;
  let router: Router;

  const mockItem: TodoItemDto = {
    id: 'abc',
    title: 'New Item',
    description: null,
    isCompleted: false,
    createdAt: '',
    completedAt: null,
  };

  beforeEach(() => {
    client = { getTodoItem: vi.fn(), createTodoItem: vi.fn(), updateTodoItem: vi.fn() };
    errorService = { extractMessage: vi.fn() };

    TestBed.configureTestingModule({
      imports: [TodoFormComponent],
      providers: [
        provideRouter([]),
        { provide: TodoItemsClient, useValue: client },
        { provide: ErrorService, useValue: errorService },
      ],
    });

    fixture = TestBed.createComponent(TodoFormComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.detectChanges();
  });

  it('should be in create mode when no id is provided', () => {
    expect(component.isEditMode()).toBe(false);
  });

  it('should have an invalid form when title is empty', () => {
    component.form.controls.title.setValue('');
    expect(component.form.invalid).toBe(true);
  });

  it('should have a valid form when title is provided', () => {
    component.form.controls.title.setValue('Some task');
    expect(component.form.valid).toBe(true);
  });

  it('should not submit when form is invalid', () => {
    component.form.controls.title.setValue('');
    component.submit();
    expect(client.createTodoItem).not.toHaveBeenCalled();
  });

  it('should call createTodoItem and navigate on valid submission', () => {
    (client.createTodoItem as ReturnType<typeof vi.fn>).mockReturnValue(of(mockItem));
    component.form.controls.title.setValue('New Task');

    component.submit();

    expect(client.createTodoItem).toHaveBeenCalledWith({ title: 'New Task', description: null });
    expect(router.navigate).toHaveBeenCalledWith(['/todo-items']);
  });

  it('should set errorMessage and clear isSubmitting on create failure', () => {
    (client.createTodoItem as ReturnType<typeof vi.fn>).mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 400 }))
    );
    (errorService.extractMessage as ReturnType<typeof vi.fn>).mockReturnValue('Validation error');
    component.form.controls.title.setValue('Task');

    component.submit();

    expect(component.errorMessage()).toBe('Validation error');
    expect(component.isSubmitting()).toBe(false);
  });

  it('should navigate back on cancel', () => {
    component.cancel();
    expect(router.navigate).toHaveBeenCalledWith(['/todo-items']);
  });
});

describe('TodoFormComponent (edit mode)', () => {
  let fixture: ComponentFixture<TodoFormComponent>;
  let component: TodoFormComponent;
  let client: Pick<TodoItemsClient, 'getTodoItem' | 'createTodoItem' | 'updateTodoItem'>;

  const existingItem: TodoItemDto = {
    id: 'xyz',
    title: 'Existing Task',
    description: 'Some notes',
    isCompleted: false,
    createdAt: '',
    completedAt: null,
  };

  beforeEach(() => {
    client = { getTodoItem: vi.fn(), createTodoItem: vi.fn(), updateTodoItem: vi.fn() };
    (client.getTodoItem as ReturnType<typeof vi.fn>).mockReturnValue(of(existingItem));

    TestBed.configureTestingModule({
      imports: [TodoFormComponent],
      providers: [
        provideRouter([]),
        { provide: TodoItemsClient, useValue: client },
        { provide: ErrorService, useValue: { extractMessage: vi.fn() } },
      ],
    });

    fixture = TestBed.createComponent(TodoFormComponent);
    component = fixture.componentInstance;

    // Bind the route :id param to the signal input before detectChanges triggers ngOnInit
    fixture.componentRef.setInput('id', 'xyz');
    fixture.detectChanges();
  });

  it('should be in edit mode when id is provided', () => {
    expect(component.isEditMode()).toBe(true);
  });

  it('should populate the form with the loaded item', () => {
    expect(component.form.controls.title.value).toBe('Existing Task');
    expect(component.form.controls.description.value).toBe('Some notes');
  });

  it('should call updateTodoItem on submission in edit mode', () => {
    (client.updateTodoItem as ReturnType<typeof vi.fn>).mockReturnValue(of(existingItem));
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    component.form.controls.title.setValue('Updated Task');
    component.submit();

    expect(client.updateTodoItem).toHaveBeenCalledWith(
      'xyz',
      expect.objectContaining({ title: 'Updated Task' })
    );
    expect(router.navigate).toHaveBeenCalledWith(['/todo-items']);
  });
});


  let router: Router;
