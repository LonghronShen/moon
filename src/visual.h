/*
 * visual.h
 *
 * Author:
 *   Miguel de Icaza (miguel@novell.com)
 *
 * Copyright 2007 Novell, Inc. (http://www.novell.com)
 *
 * See the LICENSE file included with the distribution for details.
 * 
 */

#ifndef __MOON_VISUAL_H__
#define __MOON_VISUAL_H__

#include "dependencyobject.h"

class TimeManager;

class Visual : public DependencyObject {
 protected:
	virtual ~Visual () {}

 public:
	Visual () : visual_parent (NULL) {};
	virtual Type::Kind GetObjectType () { return Type::VISUAL; };	

	//
	// InsideObject:
	//   Returns whether the position x, y is inside the object
	//
	virtual bool InsideObject (cairo_t *cr, double x, double y);
	
	virtual TimeManager *GetTimeManager ();
	
	void SetVisualParent (UIElement* visual_parent) { this->visual_parent = visual_parent; }
	UIElement* GetVisualParent () { return visual_parent; }

private:
	UIElement *visual_parent;
};


G_BEGIN_DECLS

void visual_set_surface (Visual* visual, Surface* surface);

G_END_DECLS

#endif /* __MOON_VISUAL_H__ */
